using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    public class MerchantItemEntry : MonoBehaviour
    {
        /** The underlying entry data. */
        private InventoryEntry _entry;

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
                Refresh(_entry);
            }
        }

        /** Can this entry have a transaction performed on it? */
        public bool CanPerformTransaction { get; private set; }

        /**
         * Initializes this entry.
         * <param name="owner">The menu that owns this entry.</param>
         * <param name="entry">The inventory index of the entry being displayed.</param>
         * <param name="buying">Is this entry being bought? If false, it is being sold.</param>
         */
        public void Initialize(MerchantMenu owner, InventoryEntry entry, bool buying)
        {
            if (owner == null) return;
            // Entry validity check
            if (buying && !owner.Merchant.Inventory.ContainsEntry(entry)) return;
            if (!buying && !owner.Merchant.Customer.Inventory.ContainsEntry(entry)) return;
            _owner = owner;
            _buying = buying;
            Entry = entry; // This will call refresh
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
        private void Refresh(InventoryEntry entry)
        {
            if (entry != null)
            {
                sprite.sprite = entry.Item.InventorySprite;
                sprite.color = entry.Item.SpriteTint;

                nameText.text =  $"{entry.Item.DisplayName} (x{entry.Count})";
                descriptionText.text = entry.Item.Description;
                priceText.text = (_buying ? "Buy $" : "Sell $") + entry.Item.Value;
                
                CanPerformTransaction = !_buying || entry.Item.Value <= _owner.Wallet;
                buyButton.interactable = CanPerformTransaction;
                // Strikethrough options that are not valid
                if (CanPerformTransaction) priceText.fontStyle &= ~FontStyles.Strikethrough;
                else  priceText.fontStyle |= FontStyles.Strikethrough;
                
                disableTintPanel.enabled = !CanPerformTransaction;

                if (null != quantityText)
                {
                    if (entry is ComputerPartInstance)
                    {
                        quantityText.gameObject.SetActive(false);
                    }
                    else if (entry is ItemStack stack)
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
            bool success = (Entry is ItemStack stack) ?
                _owner.Merchant.Customer.Inventory.TryAddItem(stack.Item) :
                _owner.Merchant.Customer.Inventory.TryAddEntry(Entry, out _);
            
            if (success && _owner.Merchant.Inventory.TryRemoveCountFromEntry(Entry, 1))
            {
                _owner.Merchant.Customer.Wallet -= Entry.Item.Value;
                GameManager.GameState.ItemsBought++;
                // TODO: Play buy sound
            }
        }

        private void Sell()
        {
            bool success = (Entry is ItemStack stack) ?
                _owner.Merchant.Inventory.TryAddItem(stack.Item) :
                _owner.Merchant.Inventory.TryAddEntry(Entry, out _);
            
            if (success && _owner.Merchant.Customer.Inventory.TryRemoveCountFromEntry(Entry, 1))
            {
                _owner.Merchant.Customer.Wallet += Entry.Item.Value;
                GameManager.GameState.ItemsSold++;
                // TODO: Play sell sound
            }
        }
    }
}