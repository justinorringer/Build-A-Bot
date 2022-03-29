using UnityEngine;

namespace BuildABot
{

    /**
     * An item's classification.
     */
    public enum EItemType
    {
        ComputerPart,
        RepairTool,
        Misc,
        KeyItem
    }
    
    /**
     * The base type of all items that exist in the game. Any concrete instance should be initialized using a derived type.
     * All item types are ScriptableObjects that can be created in the editor.
     */
    public abstract class Item : ScriptableObject
    {
        [Tooltip("The name of this item that is displayed in-game.")]
        [SerializeField] private string displayName;
        [Tooltip("The description of this item that is displayed in-game.")]
        [TextArea]
        [SerializeField] private string description;

        [Tooltip("The sprite used by the item when displayed in the inventory.")]
        [SerializeField] private Sprite inventorySprite;

        [Tooltip("The sprite used by item instances displayed in the overworld.")]
        [SerializeField] private Sprite overworldSprite;

        [Tooltip("The base value of this item when selling to or buying from the trader.")]
        [SerializeField] private int value;
        
    // Public facing properties

        /** The displayed name of this item (read-only). */
        public string DisplayName => displayName;
        /** The description of this item (read-only). */
        public string Description => description;

        /** The type of this item. */
        public abstract EItemType Type { get; }
        
        /** The sprite used by this item in the inventory (read-only). */
        public Sprite InventorySprite => inventorySprite;
        /** The sprite used by this item in the overworld (read-only). */
        public Sprite OverworldSprite => overworldSprite;

        /** The value of this item when being bought from or sold to the trader (read-only). */
        public int Value => value;
        
    // Flags

        /** Can this item be sold? */
        public virtual bool Sellable => true;

        /** Can this item be dropped, discarded, or otherwise removed from the player inventory? */
        public virtual bool Removable => true;
    }

    /**
     * The base type used for all item types that can be placed in a stack.
     */
    public abstract class StackableItem : Item
    {

        [Tooltip("The number of this item that can be stored in a single stack. Unused for durability based items.")]
        [SerializeField] private int stackSize = 100;

        /** The number of times this item can be stack before overflowing to a new stack. Unused by items with durability. */
        public int StackSize => stackSize;
    }

}