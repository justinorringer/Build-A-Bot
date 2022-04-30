using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BuildABot
{

    /**
     * A wrapper for item stacks that pairs an item with a quantity.
     */
    [Serializable]
    public class ItemStack : InventoryEntry
    {

        [Tooltip("The item to stack.")]
        [SerializeField] private StackableItem item;
        
        [Tooltip("The quantity of the item in this stack.")]
        [Min(1)]
        [SerializeField] private int count;

        /** The item being stacked. */
        public override Item Item => item;
        
        /** The quantity of the item in this stack. */
        public override int Count => count;

        /** The maximum count that this stack can hold. */
        public int Capacity => item.StackSize;

        /**
         * Constructs a new item stack from the given item and count.
         */
        public ItemStack(StackableItem item, int count)
        {
            if (count < 1) throw new ArgumentException("New item stacks must have a positive count.", nameof(count));
            if (null == item) throw new ArgumentNullException(nameof(item),"Item stacks should not contain null items.");
            this.item = item;
            this.count = count;
        }

        /**
         * Tries to add the given quantity to this stack. If the amount can be added without overflow
         * zero is returned, otherwise this will return the number that could not fit in this stack. Any portion that
         * can fit in this stack will be added to this stack's count.
         * <param name="quantity">The number to attempt to add to this stack.</param>
         * <returns>The number that could not be added to this stack, 0 implies a fully successful add.</returns>
         */
        public int TryAdd(int quantity)
        {
            int delta = Mathf.Min(quantity, Capacity - Count);
            count += delta;
            if (delta != 0) ApplyChanges();
            return quantity - delta;
        }

        /**
         * Tries to remove the given quantity from this stack. If the stack does not contain enough to fully supply
         * the requested amount then this stack will not change and false is returned. Otherwise, the amount specified
         * is removed and true is returned.
         * <param name="quantity">The amount to try to remove from this stack.</param>
         * <returns>True if the amount could be removed, false otherwise.</returns>
         */
        public bool TryRemove(int quantity)
        {
            bool success = count >= quantity;
            if (success)
            {
                count -= quantity;
                ApplyChanges();
            }
            return success;
        }
    }
    
    /**
     * A single instance of a computer part item with its own durability value and modifiers.
     * Note that item instances are meant for unique items with individual stats and traits and as
     * such will not stack.
     */
    [Serializable]
    public class ComputerPartInstance : InventoryEntry
    {
        [Tooltip("The item to of this entry.")]
        [SerializeField] private ComputerPartItem item;
        
        [Tooltip("The current durability of this item.")]
        [SerializeField] private int durability;

        /** The base item used by this instance. */
        public override Item Item => item;

        /** The typed item of this instance. */
        public ComputerPartItem ComputerPartItem => item;

        public override int Count => 1;

        public override bool CanEquip => true;

        /** The current durability of this part. */
        public int Durability => durability;

        /** The maximum possible durability of this part. */
        public int MaxDurability => item.MaxDurability;

        /**
         * Constructs a new Item Instance with full durability.
         * <param name="baseItem">The base item data to use when generating this instance.</param>
         */
        public ComputerPartInstance(ComputerPartItem baseItem)
        {
            item = baseItem;
            durability = baseItem.MaxDurability;
        }

        /**
         * Generates an instance of the provided base item with a random durability value. 
         * The generated durability value will be clamped between 0 and the max durability of the base item as 
         * well as between the provided min an max values if specified.
         * <param name="baseItem">The item to generate the instance from.</param>
         * <param name="min">
         * The minimum value of the durability to assign. Must be between 0 and the max durability of the item.
         * </param>
         * <param name="max">
         * The maximum value of the durability to assign. Must be between 0 and the max durability of the item.
         * If not specified or less than 0, the default max item durability will be used.
         * </param>
         */
        public static ComputerPartInstance GenerateInstanceRndDurability(ComputerPartItem baseItem, int min = 0, int max = -1)
        {
            int upperBound = baseItem.MaxDurability;
            max = max < 0 ? upperBound : max;
            int clampedMin = Mathf.Clamp(min, 0, upperBound);
            int clampedMax = Mathf.Clamp(max, clampedMin, upperBound);
            ComputerPartInstance result = new ComputerPartInstance(baseItem)
            {
                durability = Random.Range(clampedMin, clampedMax + 1)
            };
            return result;
        }

        /**
         * Applies damage to the durability of this item instance. Durability cannot go below zero.
         * <param name="amount">The amount of durability to remove from this item. This may be modified by skills or abilities.</param>
         */
        public void ApplyDamage(int amount)
        {
            durability -= amount; // TODO: Apply modifiers from skills/perks to the parameter before calling this function
            durability = (durability >= 0) ? durability : 0;
            ApplyChanges();
        }
    }

    /**
     * An inventory entry for a key item.
     */
    [Serializable]
    public class KeyItemEntry : InventoryEntry
    {
        private KeyItem _baseItemData;

        public override Item Item => _baseItemData;

        public override int Count => 1;

        /**
         * Constructs a new Key Item inventory entry.
         * <param name="item">The base key item data to represent.</param>
         */
        public KeyItemEntry(KeyItem item)
        {
            _baseItemData = item;
        }
    }
}