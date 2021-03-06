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
                Refresh(_entry);
            }
        }

        /**
         * Updates the slot display using the bound entry data.
         */
        public void Refresh()
        {
            Refresh(_entry);
        }

        /**
         * Updates the slot display.
         */
        private void Refresh(InventoryEntry entry)
        {
            if (entry != null)
            {
                sprite.sprite = entry.Item.InventorySprite;
                sprite.color = entry.Item.SpriteTint;
                
                unequippedButton.gameObject.SetActive(!entry.Equipped);
                equippedButton.gameObject.SetActive(entry.Equipped);

                unequippedButton.interactable = true;
                equippedButton.interactable = true;
                
                if (entry is ComputerPartInstance cp)
                {
                    durabilityBar.gameObject.SetActive(cp.Durability != cp.MaxDurability);
                    durabilityBar.value = cp.Durability / (float) cp.MaxDurability;
                    quantityText.gameObject.SetActive(false);
                }
                else if (entry is ItemStack stack)
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
            InventoryMenu.ActiveSlot = this;
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