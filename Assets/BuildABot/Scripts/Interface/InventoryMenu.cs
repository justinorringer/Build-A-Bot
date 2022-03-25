using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class InventoryMenu : MonoBehaviour
    {
        [Tooltip("The player instance using this menu.")]
        [SerializeField] private Player player;
        
        [Tooltip("The main menu component that owns the inventory menu.")]
        [SerializeField] private MainMenu mainMenu;

        [Tooltip("The prefab inventory UI object used for the item slots.")]
        [SerializeField] private InventoryMenuItemSlot inventorySlotPrefab;

        [Tooltip("The RectTransform used by the inventory menu.")]
        [SerializeField] private RectTransform inventoryRect;
        [Tooltip("The RectTransform used by the items panel.")]
        [SerializeField] private RectTransform itemsPanel;
        [Tooltip("the grid layout used to place the inventory slots on the screen.")]
        [SerializeField] private GridLayoutGroup itemsGridLayout;

        [Tooltip("The details panel used to display item information.")]
        [SerializeField] private InventoryMenuItemDetails detailsPanel;

        [Tooltip("The number of item slots shown in each row.")]
        [SerializeField] private int slotsPerRow = 4;

        [Tooltip("The size of the slot padding compared to the slots themselves.")]
        [SerializeField] private float paddingRatio = 0.25f;

        /** The list of inventory slots spawned at runtime. Cleared on disable. */
        private List<GameObject> _spawnedSlots;

        /** The details panel used to display item information. */
        public InventoryMenuItemDetails DetailsPanel => detailsPanel;

        /** The player instance controlling this menu. */
        public Player Player => player;

        /**
         * Generates the slots in the UI.
         */
        private void GenerateSlots()
        {
            int slots = player.Attributes.InventorySpace.CurrentValue;
            float relativeWidth = slotsPerRow + (slotsPerRow + 1) * paddingRatio;
            
            float width = (itemsPanel.anchorMax.x - itemsPanel.anchorMin.x) * (inventoryRect.anchorMax.x - inventoryRect.anchorMin.x);

            float slotSizeRelative = width / relativeWidth;
            
            float slotSize = slotSizeRelative * Screen.width;
            float paddingSize = slotSize * paddingRatio;

            int paddingSizePixels = (int) paddingSize;

            // Update the grid layout
            itemsGridLayout.padding = new RectOffset(paddingSizePixels, paddingSizePixels, paddingSizePixels, paddingSizePixels);
            itemsGridLayout.spacing = new Vector2(paddingSize, paddingSize);
            itemsGridLayout.cellSize = new Vector2(slotSize, slotSize);
            itemsGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            itemsGridLayout.constraintCount = slotsPerRow;

            // Add the slot prefabs
            Transform layoutParent = itemsGridLayout.gameObject.transform;
            ReadOnlyCollection<InventoryEntry> entries = player.Inventory.Entries;
            for (int i = 0; i < slots; i++)
            {
                InventoryMenuItemSlot slot = Instantiate(inventorySlotPrefab, layoutParent);
                slot.Entry = i < entries.Count ? entries[i] : null;
                slot.InventoryMenu = this;
                _spawnedSlots.Add(slot.gameObject);
            }
        }

        protected void OnEnable()
        {
            _spawnedSlots = new List<GameObject>();
            DetailsPanel.InventoryMenu = this;
            GenerateSlots();
        }

        protected void OnDisable()
        {
            foreach (GameObject slot in _spawnedSlots)
            {
                Destroy(slot);
            }

            DetailsPanel.Entry = null;
        }
    }
}
