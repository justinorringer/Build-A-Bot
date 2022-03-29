using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class InventoryMenuItemSlot : MonoBehaviour
    {

        /** The underlying entry data. */
        private InventoryEntry _entry;

        [Tooltip("The slider used to display durability.")]
        [SerializeField] private Slider durabilityBar;

        [Tooltip("The text object used to show item quantity.")]
        [SerializeField] private TMP_Text quantityText;
        
        [Tooltip("The image used to display the item sprite.")]
        [SerializeField] private Image sprite;
        
        /** The inventory menu that owns this slot object. */
        public InventoryMenu InventoryMenu { get; set; }

        /** The entry used by this item slot. */
        public InventoryEntry Entry
        {
            get => _entry;
            set
            {
                _entry = value;
                Initialize();
            }
        }

        /**
         * Initializes the slot display.
         */
        private void Initialize()
        {
            if (Entry != null)
            {
                sprite.sprite = Entry.Item.InventorySprite;
                sprite.color = Color.white;
                if (Entry is ComputerPartInstance cp)
                {
                    durabilityBar.gameObject.SetActive(true);
                    durabilityBar.value = cp.Durability / (float) cp.MaxDurability;
                    quantityText.gameObject.SetActive(false);
                }
                else if (Entry is ItemStack stack)
                {
                    durabilityBar.gameObject.SetActive(false);
                    quantityText.gameObject.SetActive(true);
                    quantityText.text = stack.Count.ToString();
                }
                else
                {
                    durabilityBar.gameObject.SetActive(false);
                    quantityText.gameObject.SetActive(false);
                }
            }
            else
            {
                sprite.sprite = null;
                sprite.color = Color.clear;
                durabilityBar.gameObject.SetActive(false);
                quantityText.gameObject.SetActive(false);
            }
        }

        /**
         * Shows this inventory entry's information in the inventory menu details panel.
         */
        public void ShowInDetailsPanel()
        {
            InventoryMenu.DetailsPanel.Entry = Entry;
        }
    }
}