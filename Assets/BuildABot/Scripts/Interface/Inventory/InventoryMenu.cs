using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;
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

        [Tooltip("The display area for the actively selected item.")]
        [SerializeField] private Image activeItemDisplay;

        [Tooltip("The number of item slots shown in each row.")]
        [SerializeField] private int slotsPerRow = 4;

        [Tooltip("The size of the slot padding compared to the slots themselves.")]
        [SerializeField] private float paddingRatio = 0.25f;

        [Tooltip("The sound to play when an item is equipped.")]
        [SerializeField] private AudioClip equipSound;

        [Tooltip("The sound to play when an item is unequipped.")]
        [SerializeField] private AudioClip unequipSound;

        /** The list of inventory slots spawned at runtime. Cleared on disable. */
        private List<InventoryMenuItemSlot> _spawnedSlots;

        /** Audio source used to play inventory sound effects. */
        private AudioSource _audio;

        /** The details panel used to display item information. */
        private InventoryMenuItemDetails DetailsPanel => detailsPanel;
        
        /** The active inventory slot being shown to the user. */
        public InventoryMenuItemSlot ActiveSlot
        {
            get => DetailsPanel.Slot;
            set
            {
                DetailsPanel.Slot = value;
                if (value == null)
                {
                    activeItemDisplay.sprite = null;
                    activeItemDisplay.gameObject.SetActive(false);
                }
                else
                {
                    activeItemDisplay.sprite = value.Entry.Item.InventorySprite;
                    activeItemDisplay.gameObject.SetActive(true);
                }
            }
        }

        /** The player instance controlling this menu. */
        public Player Player => player;

        public void Start()
        {
            _audio = GetComponent<AudioSource>();
        }

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
                _spawnedSlots.Add(slot);
            }

            if (_spawnedSlots.Count > 0) _spawnedSlots[0].Select();
        }

        protected void OnEnable()
        {
            _spawnedSlots = new List<InventoryMenuItemSlot>();
            DetailsPanel.InventoryMenu = this;
            GenerateSlots();
            player.PlayerController.InputActions.UI.Back.performed += Input_Back;
        }

        protected void OnDisable()
        {
            foreach (InventoryMenuItemSlot slot in _spawnedSlots)
            {
                Destroy(slot.gameObject);
            }

            ActiveSlot = null;
            
            player.PlayerController.InputActions.UI.Back.performed -= Input_Back;
        }

        private void Input_Back(InputAction.CallbackContext context)
        {
            if (ActiveSlot != null)
            {
                InventoryMenuItemSlot slot = ActiveSlot;
                ActiveSlot = null; // Exit Detail panel before trying to return to main menu
                slot.Select();
            }
            else
            {
                mainMenu.ReturnToLandingPage();
            }
        }

        public void PlayEquipSound()
        {
            if (_audio != null && equipSound != null)
                _audio.PlayOneShot(equipSound);
        }

        public void PlayUnequipSound()
        {
            if (_audio != null && unequipSound != null)
                _audio.PlayOneShot(unequipSound);
        }
    }
}
