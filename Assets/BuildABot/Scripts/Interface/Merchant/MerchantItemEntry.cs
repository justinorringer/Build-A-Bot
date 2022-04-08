using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class MerchantItemEntry : MonoBehaviour
    {
        /** The underlying entry data. */
        private InventoryEntry _entry;
        /** The inventory entry index to display. */
        private int _entryIndex;

        [Tooltip("The text object used to show the item name.")]
        [SerializeField] private TMP_Text nameText;

        [Tooltip("The text object used to show the item description.")]
        [SerializeField] private TMP_Text descriptionText;

        [Tooltip("The text object used to show the item quantity.")]
        [SerializeField] private TMP_Text quantityText;

        [Tooltip("The text object used to show the item price.")]
        [SerializeField] private TMP_Text priceText;

        [Tooltip("The text object used to show the item price.")]
        [SerializeField] private Button buyButton;
        
        [Tooltip("The image used to tint this entry when buying is disabled.")]
        [SerializeField] private Image disableTintPanel;
        
        [Tooltip("The image used to display the item sprite.")]
        [SerializeField] private Image sprite;

        /** The TraderMenu that owns this entry. */
        private MerchantMenu _owner;

        /** Is this entry being bought? If false, it is being sold. */
        private bool _buying;

        /** The entry used by this item entry. */
        private InventoryEntry Entry
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
         * Initializes this entry.
         * <param name="owner">The menu that owns this entry.</param>
         * <param name="entry">The inventory index of the entry being displayed.</param>
         * <param name="buying">Is this entry being bought? If false, it is being sold.</param>
         */
        public void Initialize(MerchantMenu owner, int entry, bool buying)
        {
            if (owner == null || entry < 0) return;
            _owner = owner;
            _buying = buying;
            _entryIndex = entry;
            Entry = _buying ?
                _owner.Merchant.Inventory.Entries[entry] :
                _owner.Merchant.Customer.Inventory.Entries[entry]; // This will refresh
        }

        /**
         * Selects this entry.
         */
        public void Select()
        {
            buyButton.Select();
        }

        /**
         * Updates the entry display.
         */
        private void Refresh()
        {
            if (Entry != null)
            {
                sprite.sprite = Entry.Item.InventorySprite;
                sprite.color = Color.white;

                nameText.text = Entry.Item.DisplayName;
                descriptionText.text = Entry.Item.Description;
                priceText.text = (_buying ? "Buy $" : "Sell $") + (Entry.Item.Value * Entry.Count); // TODO: Handle buying and selling a single item
                
                bool canBuy = Entry.Item.Value < _owner.Wallet;
                buyButton.interactable = canBuy;
                disableTintPanel.enabled = !canBuy;

                if (null != quantityText)
                {
                    if (Entry is ComputerPartInstance)
                    {
                        quantityText.gameObject.SetActive(false);
                    }
                    else if (Entry is ItemStack stack)
                    {
                        quantityText.gameObject.SetActive(true);
                        quantityText.text = stack.Count.ToString();
                    }
                    else
                    {
                        quantityText.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                sprite.sprite = null;
                sprite.color = Color.clear;
                if (null != quantityText) quantityText.gameObject.SetActive(false);
                priceText.text = "-";
            }
        }

        /**
         * Performs the transaction for this entry.
         */
        public void PerformTransaction()
        {
            if (_buying) Buy();
            else Sell();
        }

        private void Buy()
        {
            _owner.Merchant.Customer.Inventory.TryAddEntry(Entry, out int overflow);
            int bought = Entry.Count - overflow;
            _owner.Merchant.Inventory.TryRemoveCountFromEntry(_entryIndex, bought);
            _owner.Merchant.Customer.Wallet -= Entry.Item.Value * bought;

            if (bought != 0)
            {
                // TODO: Play buy sound
            }
        }

        private void Sell()
        {
            _owner.Merchant.Inventory.TryAddEntry(Entry, out int overflow);
            int sold = Entry.Count - overflow;
            _owner.Merchant.Customer.Inventory.TryRemoveCountFromEntry(_entryIndex, sold);

            if (sold != 0)
            {
                // TODO: Play sell sound
            }
        }
    }
}