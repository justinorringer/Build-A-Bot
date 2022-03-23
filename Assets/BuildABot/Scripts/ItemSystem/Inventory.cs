using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;

namespace BuildABot
{

    /**
     * The base type used to represent inventory item entries.
     */
    public abstract class InventoryEntry
    {
        /** The item stored in this inventory entry. */
        public abstract Item Item { get; }
        
        /** The stack count of this entry if it is a stack. */
        public abstract int Count { get; }
    }
    
    /**
     * An inventory that can contain an specified number of items.
     */
    public class Inventory : MonoBehaviour
    {

        /** The runtime collection of entries inside of this inventory. */
        private readonly List<InventoryEntry> _entries = new List<InventoryEntry>();
        
        [Tooltip("The maximum number of entries that can be stored in this inventory.")]
        [SerializeField] private int maxSlots = 10;

        // TODO: Add support for adding/removing listeners at runtime
        
        [Tooltip("A dispatcher called whenever an item is added to this inventory. Subscribers will receive the added item and count.")]
        [SerializeField] private UnityEvent<Item, int> onItemAdded;
        [Tooltip("A dispatcher called whenever an item is removed from this inventory. Subscribers will receive the removed item and count.")]
        [SerializeField] private UnityEvent<Item, int> onItemRemoved;
        
        [Tooltip("A dispatcher called whenever a new entry slot is filled in this inventory. Subscribers will receive the entry and its index.")]
        [SerializeField] private UnityEvent<InventoryEntry, int> onEntryAdded;
        [Tooltip("A dispatcher called whenever an entry is removed from a slot in this inventory. Subscribers will receive the entry and its index.")]
        [SerializeField] private UnityEvent<InventoryEntry> onEntryRemoved;
        [Tooltip("A dispatcher called whenever this inventory modifies an entry. Subscribers will receive the entry and its index.")]
        [SerializeField] private UnityEvent<InventoryEntry, int> onEntryModified;
        
        /** Gets the (read-only) list of item entries in this inventory. */
        public ReadOnlyCollection<InventoryEntry> Entries => _entries.AsReadOnly();
        
        /** Gets the number of item slots available in this inventory. */
        public int MaxSlots => maxSlots;

        /**
         * Updates the number of slots available in this inventory.
         * <param name="slots">The new number of slots that this inventory can hold.</param>
         */
        public void UpdateSlotCount(int slots)
        {
            Debug.Assert(slots > 0, "An inventory must have at least one slot.");
            maxSlots = slots;
        }

        /**
         * Checks whether or not this inventory contains at least the provided quantity of the specified item.
         * If not specified, the requested quantity will be 1.
         * <param name="item">The item to search for.</param>
         * <param name="quantity">The number to search for taken from all stacks and unique instances.</param>
         * <returns>True if the given quantity is found.</returns>
         */
        public bool HasItem(Item item, int quantity = 1)
        {
            int found = 0;
            foreach (InventoryEntry entry in _entries)
            {
                if (entry.Item == item) found += entry.Count;

                if (found >= quantity) break;
            }
            
            return found >= quantity;
        }

        /**
         * Adds the provided inventory entry if space exists. If the entry is stackable, it will automatically stack
         * with existing entries.
         * <param name="entry">The entry to try to add.</param>
         * <param name="overflow">(Out) The amount of the requested entry addition could not be added due to slot or stack size limitations.</param>
         * <returns>True if the entry could be fully added, false otherwise.</returns>
         */
        public bool TryAddEntry(InventoryEntry entry, out int overflow)
        {
            if (entry is ItemStack stack) // Entry is stackable
            {
                // Handle increasing stack size if a stack already exists
                
                int remaining = entry.Count;
                for (int i = 0; i < _entries.Count; i++)
                {
                    InventoryEntry e = _entries[i];
                    if (entry.Item == e.Item && e is ItemStack s)
                    {
                        int delta = remaining;
                        // Add the remaining amount to the stack or fill the stack and continue
                        remaining = s.TryAdd(remaining);
                        delta -= remaining;
                        // Fire events
                        if (delta != 0) onEntryModified.Invoke(e, i);
                    }

                    if (remaining == 0)
                    {
                        overflow = 0;
                        // Fire events
                        onItemAdded.Invoke(entry.Item, stack.Count);
                        return true; // Everything has been added, finished
                    }
                }
            
                // Could not add everything, try to create stacks to handle the remaining items
                
                while (_entries.Count < maxSlots && remaining > 0)
                {
                    int delta = Mathf.Min(remaining, stack.Capacity);
                    InventoryEntry e = new ItemStack(entry.Item as StackableItem, delta);
                    _entries.Add(e);
                    // Fire events
                    onEntryAdded.Invoke(e, _entries.Count - 1);
                    remaining -= delta;
                }
                overflow = remaining;
                // Fire events
                onItemAdded.Invoke(entry.Item, stack.Count - remaining);
                return remaining == 0;
            }
            else
            {
                bool canAdd = _entries.Count < maxSlots;
                if (canAdd)
                {
                    _entries.Add(entry);
                    // Fire events
                    onEntryAdded.Invoke(entry, _entries.Count - 1);
                    onItemAdded.Invoke(entry.Item, 1);
                }
                overflow = canAdd ? 0 : 1;
                return canAdd;
            }
        }

        /**
         * Tries to add the provided amount of a stackable item to this inventory. This version returns overflow feedback.
         * <param name="item">The item to add.</param>
         * <param name="count">The amount to add.</param>
         * <param name="overflow">The amount that could not fit in the inventory.</param>
         * <returns>True if the operation was fully successful, false otherwise.</returns>
         */
        public bool TryAddItem(StackableItem item, int count, out int overflow)
        {
            return TryAddEntry(new ItemStack(item, count), out overflow);
        }
        
        /**
         * Tries to add the provided amount of a stackable item to this inventory.
         * <param name="item">The item to add.</param>
         * <param name="count">The amount to add.</param>
         * <returns>True if the operation was fully successful, false otherwise.</returns>
         */
        public bool TryAddItem(StackableItem item, int count)
        {
            return TryAddEntry(new ItemStack(item, count), out int overflow);
        }

        /**
         * Tries to add one instance of the provided item to this inventory.
         * <param name="item">The item to add.</param>
         * <returns>True if the operation was successful.</returns>
         */
        public bool TryAddItem(Item item)
        {
            if (item is StackableItem s) return TryAddItem(s, 1, out int o1);
            if (item is ComputerPartItem c) return TryAddEntry(new ComputerPartInstance(c), out int o2);
            if (item is KeyItem k) return TryAddEntry(new KeyItemEntry(k), out int o2);
            return false;
        }
        
        // TODO: Add searching for specific entry by Item for removal

        /**
         * Attempts to remove the specified item count from the entry at the provided index. If the entry does not have
         * a sufficient count to fulfill the request the operation will fail and false is returned. If the entry is for
         * a non-stackable item the count must be 1 for the operation to succeed.
         * Success implies that the requested entry has been modified or fully removed.
         * <param name="index">The index of the entry to modify.</param>
         * <param name="count">The amount to try to remove. this must be 1 for non-stackable types.</param>
         * <returns>True if the operation was successful, false otherwise.</returns>
         */
        public bool TryRemoveCountFromEntry(int index, int count)
        {
            bool result = false;
            if (index >= 0 && index < _entries.Count) // Bounds check
            {
                if (count > 1 && _entries[index] is ItemStack stack) // Handle stackable items
                {
                    result = stack.TryRemove(count);
                    // If successful and the stack was emptied remove the entry
                    if (result && stack.Count == 0) RemoveEntryByIndex(index, true);
                    else if (result) onEntryModified.Invoke(_entries[index], index);
                }
                else if (_entries[index] is ComputerPartInstance c || _entries[index] is KeyItemEntry k) // Handle non-stackable items
                {
                    result = count == 1;
                    // Only remove the entry if the request was valid
                    if (result) RemoveEntryByIndex(index, true);
                }
            }

            return result;
        }

        /**
         * Removes the inventory entry with the provided index. This requires that the 
         * <param name="index">The index of the entry to remove.</param>
         * <param name="force">Should this operation bypass the removable check on the item? Defaults to false.</param>
         * <returns>The removed entry if it could be removed.</returns>
         */
        public InventoryEntry RemoveEntryByIndex(int index, bool force = false)
        {
            if (index < 0 || index >= _entries.Count) return null; // Check index bounds
            // check that the item can be removed or force the operation
            if (force || _entries[index].Item.Removable)
            {
                InventoryEntry removed = _entries[index];
                _entries.RemoveAt(index);
                // Fire events
                onEntryRemoved.Invoke(removed);
                onItemRemoved.Invoke(removed.Item, (removed is ItemStack s) ? s.Count : 1);
                return removed;
            }
            // Could not be removed, return null
            return null;
        }

        /**
         * Gets a filtered list of all entries in this inventory using a predicate to match valid entries.
         * <param name="match">the matching predicate to use. Should return true for a given entry to include it in the output.</param>
         * <returns>A list of matching entries.</returns>
         */
        public List<InventoryEntry> FilterEntries(Predicate<InventoryEntry> match)
        {
            return _entries.FindAll(match);
        }

        /**
         * Sorts this inventory by the name of each item.
         */
        public void SortByName()
        {
            _entries.Sort((a, b) =>
                    String.Compare(a.Item.DisplayName, b.Item.DisplayName, StringComparison.Ordinal)
                    );
        }

        /**
         * Sorts this inventory by the type of each item.
         */
        public void SortByType()
        {
            _entries.Sort((a, b) =>
                a.Item.Type.CompareTo(b.Item.Type)
            );
        }
    }
}