using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    [RequireComponent(typeof(Inventory))]
    public class Merchant : InteractableCharacter
    {

        [Tooltip("The loot tables used to generate this merchant's inventory.")]
        [SerializeField] private List<LootTable> lootTables;
        
        /** The inventory of this trader. */
        public Inventory Inventory { get; private set; }
        
        /** The current customer being served. */
        public Player Customer { get; private set; }

        protected void Awake()
        {
            Inventory = GetComponent<Inventory>();
            if (lootTables.Count > 0)
            {
                // Populate with a list of random items
                List<InventoryEntry> items = lootTables[Mathf.Clamp(GameManager.GameState.GameStage, 0, lootTables.Count - 1)].GenerateItemList();
                foreach (InventoryEntry entry in items)
                {
                    Inventory.TryAddEntry(entry, out _);
                }
            }
        }
        
        protected override void OnInteract(InteractionController instigator)
        {
            Customer = instigator.Player;
            base.OnInteract(instigator);
        }

        protected override void OnFinishDialogue(Dialogue finished, DialogueSpeaker speaker)
        {
            base.OnFinishDialogue(finished, speaker);
            Customer = null;
        }
    }
}