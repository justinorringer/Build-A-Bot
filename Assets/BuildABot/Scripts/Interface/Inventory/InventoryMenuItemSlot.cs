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

        [Tooltip("The unequipped version of the inventory button.")]
        [SerializeField] private Button unequippedButton;

        [Tooltip("The equipped version of the inventory button.")]
        [SerializeField] private Button equippedButton;
        
        /** The inventory menu that owns this slot object. */
        public InventoryMenu InventoryMenu { get; set; }

        /** The entry used by this item slot. */
        public InventoryEntry Entry
        {
            get => _entry;
            set
            {
                if (_entry != null) _entry.OnChange -= Refresh;
                _entry = value;
                if (_entry != null) _entry.OnChange += Refresh;
                Refresh();
            }
        }

        /**
         * Updates the slot display.
         */
        public void Refresh()
        {
            if (Entry != null)
            {
                sprite.sprite = Entry.Item.InventorySprite;
                sprite.color = Color.white;
                
                unequippedButton.gameObject.SetActive(!Entry.Equipped);
                equippedButton.gameObject.SetActive(Entry.Equipped);

                unequippedButton.interactable = true;
                equippedButton.interactable = true;
                
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
                
                unequippedButton.gameObject.SetActive(true);
                equippedButton.gameObject.SetActive(false);

                unequippedButton.interactable = false;
                equippedButton.interactable = false;
            }
        }

        /**
         * Shows this inventory entry's information in the inventory menu details panel.
         */
        public void ShowInDetailsPanel()
        {
            InventoryMenu.DetailsPanel.Slot = this;
        }

        /**
         * Selects the underlying button of this slot.
         */
        public void Select()
        {
            if (Entry != null && Entry.Equipped) equippedButton.Select();
            else unequippedButton.Select();
        }
    }
}