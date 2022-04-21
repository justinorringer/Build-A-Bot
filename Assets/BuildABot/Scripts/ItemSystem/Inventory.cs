using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        /** Can this item entry be equipped? */
        public virtual bool CanEquip => false;

        /** Is this entry currently equipped? */
        private bool _equipped;

        /** Is this entry currently equipped? */
        public bool Equipped
        {
            get => _equipped;
            set
            {
                if (CanEquip)
                {
                    _equipped = value;
                    if (_equipped) _onEquip.Invoke(this);
                    else _onUnequip.Invoke(this);
                    ApplyChanges();
                }
                else Debug.LogWarning("Cannot equip an item that is not marked CanEquip.");
            }
        }

        /** The event called whenever a change is applied to this entry. */
        private readonly UnityEvent<InventoryEntry> _onChange = new UnityEvent<InventoryEntry>();

        /** The event called whenever this entry is equipped. */
        private readonly UnityEvent<InventoryEntry> _onEquip = new UnityEvent<InventoryEntry>();

        /** The event called whenever this entry is unequipped. */
        private readonly UnityEvent<InventoryEntry> _onUnequip = new UnityEvent<InventoryEntry>();
        
        /** An event triggered whenever a change is applied to this entry. Sends subscribers a reference to this entry. */
        public event UnityAction<InventoryEntry> OnChange
        {
            add => _onChange.AddListener(value);
            remove => _onChange.RemoveListener(value);
        }
        
        /** The event called whenever this entry is equipped. */
        public event UnityAction<InventoryEntry> OnEquip
        {
            add => _onEquip.AddListener(value);
            remove => _onEquip.RemoveListener(value);
        }
        
        /** The event called whenever this entry is unequipped. */
        public event UnityAction<InventoryEntry> OnUnequip
        {
            add => _onUnequip.AddListener(value);
            remove => _onUnequip.RemoveListener(value);
        }

        /**
         * Handles any changes made to this entry.
         */
        protected void ApplyChanges()
        {
            _onChange.Invoke(this);
        }
    }
    
    /**
     * An inventory that can contain an specified number of items.
     */
    public class Inventory : MonoBehaviour
    {

        /** The runtime collection of entries inside of this inventory. */
        private readonly LinkedList<InventoryEntry> _entries = new LinkedList<InventoryEntry>();
        private readonly Dictionary<InventoryEntry, LinkedListNode<InventoryEntry>> _lookup = new Dictionary<InventoryEntry, LinkedListNode<InventoryEntry>>();
        
        [Tooltip("The maximum number of entries that can be stored in this inventory.")]
        [SerializeField] private int maxSlots = 10;
        
        #region Events

        [Tooltip("A dispatcher called whenever an item is added to this inventory. Subscribers will receive the added item and count.")]
        [SerializeField] private UnityEvent<Item, int> onItemAdded;
        [Tooltip("A dispatcher called whenever an item is removed from this inventory. Subscribers will receive the removed item and count.")]
        [SerializeField] private UnityEvent<Item, int> onItemRemoved;
        
        [Tooltip("A dispatcher called whenever a new entry slot is filled in this inventory. Subscribers will receive a reference to the entry.")]
        [SerializeField] private UnityEvent<InventoryEntry> onEntryAdded;
        [Tooltip("A dispatcher called whenever an entry is removed from a slot in this inventory. Subscribers will receive the entry and its index.")]
        [SerializeField] private UnityEvent<InventoryEntry> onEntryRemoved;
        [Tooltip("A dispatcher called whenever this inventory modifies an entry. Subscribers will receive a reference to the entry.")]
        [SerializeField] private UnityEvent<InventoryEntry> onEntryModified;
        
        /** An event triggered whenever an item is added to this inventory. Subscribers will receive the added item and count. */
        public event UnityAction<Item, int> OnItemAdded
        {
            add => onItemAdded.AddListener(value);
            remove => onItemAdded.RemoveListener(value);
        }
        /** An event triggered whenever an item is removed from this inventory. Subscribers will receive the removed item and count. */
        public event UnityAction<Item, int> OnItemRemoved
        {
            add => onItemRemoved.AddListener(value);
            remove => onItemRemoved.RemoveListener(value);
        }
        
        /** An event triggered whenever a new entry slot is filled in this inventory. Subscribers will receive a reference to the entry. */
        public event UnityAction<InventoryEntry> OnEntryAdded
        {
            add => onEntryAdded.AddListener(value);
            remove => onEntryAdded.RemoveListener(value);
        }
        /** An event triggered whenever an entry is removed from a slot in this inventory. Subscribers will receive a reference to the entry. */
        public event UnityAction<InventoryEntry> OnEntryRemoved
        {
            add => onEntryRemoved.AddListener(value);
            remove => onEntryRemoved.RemoveListener(value);
        }
        /** An event triggered whenever this inventory modifies an entry. Subscribers will receive the entry and its index." */
        public event UnityAction<InventoryEntry> OnEntryModified
        {
            add => onEntryModified.AddListener(value);
            remove => onEntryModified.RemoveListener(value);
        }
        
        #endregion
        
        /** Gets the (read-only) list of item entries in this inventory. */
        public ReadOnlyCollection<InventoryEntry> Entries => _entries.ToList().AsReadOnly();
        
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
         * Does this inventory contain the provided entry?
         * <param name="entry">The entry to check for</param>
         * <returns>True if the entry is in this inventory, false otherwise.</returns>
         */
        public bool ContainsEntry(InventoryEntry entry)
        {
            return entry != null && _lookup.ContainsKey(entry);
        }

        /**
         * Adds the provided inventory entry if space exists. If the entry is stackable, it will automatically stack
         * with existing entries. If the provided entry is already tracked by this inventory the operation will fail.
         * <param name="entry">The entry to try to add.</param>
         * <param name="overflow">(Out) The amount of the requested entry addition could not be added due to slot or stack size limitations.</param>
         * <returns>True if the entry could be fully added, false otherwise.</returns>
         */
        public bool TryAddEntry(InventoryEntry entry, out int overflow)
        {
            if (entry == null || _lookup.ContainsKey(entry))
            {
                overflow = 0;
                return false;
            }
            
            if (entry is ItemStack stack) // Entry is stackable
            {
                // Handle increasing stack size if a stack already exists
                
                int remaining = entry.Count;
                foreach (InventoryEntry e in _entries)
                {
                    if (entry.Item == e.Item && e is ItemStack s)
                    {
                        int delta = remaining;
                        // Add the remaining amount to the stack or fill the stack and continue
                        remaining = s.TryAdd(remaining);
                        delta -= remaining;
                        // Fire events
                        if (delta != 0) onEntryModified.Invoke(e);
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
                    LinkedListNode<InventoryEntry> node = _entries.AddLast(e);
                    _lookup.Add(e, node);
                    e.OnChange += onEntryModified.Invoke;
                    // Fire events
                    onEntryAdded.Invoke(e);
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
                    LinkedListNode<InventoryEntry> node = _entries.AddLast(entry);
                    _lookup.Add(entry, node);
                    entry.OnChange += onEntryModified.Invoke;
                    // Fire events
                    onEntryAdded.Invoke(entry);
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
         * <param name="count">The amount to add. Defaults to 1.</param>
         * <returns>True if the operation was fully successful, false otherwise.</returns>
         */
        public bool TryAddItem(StackableItem item, int count = 1)
        {
            return TryAddEntry(new ItemStack(item, count), out _);
        }

        /**
         * Tries to add the provided number of instances of the specified item to this inventory.
         * Note that for non-stackable item types multiple instances will be generated.
         * <param name="item">The item to add.</param>
         * <param name="count">The amount to add.</param>
         * <param name="overflow">The amount that could not fit in the inventory.</param>
         * <returns>True if the operation was successful.</returns>
         */
        public bool TryAddItem(Item item, int count, out int overflow)
        {
            if (item is StackableItem s) return TryAddItem(s, count, out overflow);
            if (item is ComputerPartItem c)
            {
                bool success = count > 0;
                for (int i = 0; i < count; i++)
                {
                    success &= TryAddEntry(new ComputerPartInstance(c), out overflow);
                    if (overflow > 0) return success;
                }
                overflow = 0;
                return success;
            }
            if (item is KeyItem k) return TryAddEntry(new KeyItemEntry(k), out overflow);
            
            overflow = 0;
            return false;
        }

        /**
         * Tries to add the provided number of instances of the specified item to this inventory.
         * Note that for non-stackable item types multiple instances will be generated.
         * <param name="item">The item to add.</param>
         * <param name="count">The amount to add. Defaults to 1.</param>
         * <returns>True if the operation was successful.</returns>
         */
        public bool TryAddItem(Item item, int count = 1)
        {
            return TryAddItem(item, count, out _);
        }
        
        // TODO: Add searching for specific entry by Item for removal

        /**
         * Attempts to remove the specified item count from the provided entry. If the entry does not have
         * a sufficient count to fulfill the request the operation will fail and false is returned. If the entry is for
         * a non-stackable item the count must be 1 for the operation to succeed.
         * Success implies that the requested entry has been modified or fully removed.
         * <param name="entry">The entry to modify.</param>
         * <param name="count">The amount to try to remove. this must be 1 for non-stackable types.</param>
         * <returns>True if the operation was successful, false otherwise.</returns>
         */
        public bool TryRemoveCountFromEntry(InventoryEntry entry, int count)
        {
            if (entry == null || !_lookup.ContainsKey(entry)) return false; // Check validity
            
            bool result = false;
            if (count >= 1 && entry is ItemStack stack) // Handle stackable items
            {
                result = stack.TryRemove(count);
                // If successful and the stack was emptied remove the entry
                if (result && stack.Count == 0) RemoveEntry(entry, true);
                else if (result) onEntryModified.Invoke(entry);
            }
            else if (entry is ComputerPartInstance || entry is KeyItemEntry) // Handle non-stackable items
            {
                result = count == 1;
                // Only remove the entry if the request was valid
                if (result) RemoveEntry(entry, true);
            }

            return result;
        }

        /**
         * Removes the provided inventory entry if it is owned by this inventory. This requires that the 
         * <param name="entry">The entry to remove.</param>
         * <param name="force">Should this operation bypass the removable check on the item? Defaults to false.</param>
         * <returns>The removed entry if it could be removed, null otherwise.</returns>
         */
        public InventoryEntry RemoveEntry(InventoryEntry entry, bool force = false)
        {
            if (entry == null || !_lookup.TryGetValue(entry, out LinkedListNode<InventoryEntry> node)) return null; // Check validity
            
            // check that the item can be removed or force the operation
            if (force || entry.Item.Removable)
            {
                InventoryEntry removed = entry;
                _entries.Remove(node);
                _lookup.Remove(entry);
                entry.OnChange -= onEntryModified.Invoke;
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
            List<InventoryEntry> matches = new List<InventoryEntry>();
            foreach (InventoryEntry entry in _entries)
            {
                if (match(entry)) matches.Add(entry);
            }
            return matches;
        }

        /**
         * Sorts this inventory by the name of each item.
         */
        public void SortByName()
        {
            List<InventoryEntry> entries = _entries.ToList();
            entries.Sort((a, b) =>
                    String.Compare(a.Item.DisplayName, b.Item.DisplayName, StringComparison.Ordinal)
                    );
            LinkedListNode<InventoryEntry> prev = null;
            foreach (InventoryEntry entry in entries)
            {
                if (_lookup.TryGetValue(entry, out LinkedListNode<InventoryEntry> node))
                {
                    _entries.Remove(node);
                    if (prev == null) _entries.AddFirst(node);
                    else _entries.AddAfter(prev, node);
                    prev = node;
                }
            }
        }

        /**
         * Sorts this inventory by the type of each item.
         */
        public void SortByType()
        {
            List<InventoryEntry> entries = _entries.ToList();
            entries.Sort((a, b) =>
                a.Item.Type.CompareTo(b.Item.Type)
            );
            LinkedListNode<InventoryEntry> prev = null;
            foreach (InventoryEntry entry in entries)
            {
                if (_lookup.TryGetValue(entry, out LinkedListNode<InventoryEntry> node))
                {
                    _entries.Remove(node);
                    if (prev == null) _entries.AddFirst(node);
                    else _entries.AddAfter(prev, node);
                    prev = node;
                }
            }
        }
    }
}