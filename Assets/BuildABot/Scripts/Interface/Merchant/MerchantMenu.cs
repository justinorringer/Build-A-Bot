using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BuildABot
{
    public class MerchantMenu : MonoBehaviour
    {
        [Tooltip("The merchant that owns this menu.")]
        [SerializeField] private Merchant merchant;

        [Tooltip("The text object used to display the menu title.")]
        [SerializeField] private TMP_Text titleText;

        [Tooltip("The text object used to display the wallet value.")]
        [SerializeField] private TMP_Text walletText;

        [Tooltip("The text object used to display the control tips.")]
        [SerializeField] private TMP_Text controlTipText;

        [Tooltip("The scroll area of this menu.")]
        [SerializeField] private ScrollRect scrollArea;

        [Tooltip("The minimum number of digits of currency that should be displayed.")]
        [Min(1)]
        [SerializeField] private int minCurrencyDigits = 5;

        [Tooltip("The root game object to spawn the item entries under.")]
        [SerializeField] private GameObject entriesRoot;

        [Tooltip("The entry prefab to spawn for each item displayed.")]
        [SerializeField] private MerchantItemEntry entryPrefab;

        /** The menu entries spawned by this menu. */
        private List<MerchantItemEntry> _spawnedEntries;

        /** The input state cache to restore to when this menu is closed. */
        private PlayerController.InputActionsStateCache _inputStateCache;

        /** Is the menu in buying mode? */
        private bool _buying = true;

        /** The merchant that owns this menu. */
        public Merchant Merchant => merchant;

        /** The current wallet value. */
        public int Wallet => Merchant.Customer != null ? Merchant.Customer.Wallet : 0;

        private void Refresh()
        {
            if (null != Merchant && null != Merchant.Customer)
            {
                gameObject.SetActive(true);
                UpdateWalletDisplay(Wallet, Wallet);

                titleText.text = _buying ? "Buy Items" : "Sell Items";
                controlTipText.text = Merchant.Customer.PerformStandardTokenReplacement(_buying ? 
                    "{INPUT:UI:Submit} Buy   {INPUT:UI:Back} Exit" :
                    "{INPUT:UI:Submit} Sell   {INPUT:UI:Back} Exit");
                
                // Spawn entries

                if (_spawnedEntries != null && _spawnedEntries.Count > 0)
                {
                    foreach (MerchantItemEntry entry in _spawnedEntries)
                    {
                        Destroy(entry.gameObject);
                    }
                }

                Inventory source = _buying ? Merchant.Inventory : Merchant.Customer.Inventory;
                _spawnedEntries = new List<MerchantItemEntry>();
                ReadOnlyCollection<InventoryEntry> entries = source.Entries;
                foreach (InventoryEntry e in entries)
                {
                    MerchantItemEntry entry = Instantiate(entryPrefab, entriesRoot.transform);
                    entry.Initialize(this, e, _buying);
                    _spawnedEntries.Add(entry);
                }

                if (_spawnedEntries.Count > 0)
                {
                    bool selected = false;
                    foreach (MerchantItemEntry entry in _spawnedEntries)
                    {
                        if (entry.CanPerformTransaction)
                        {
                            entry.Select();
                            selected = true;
                            break;
                        }
                    }

                    if (!selected)
                    {
                        // Select scroll bar
                        scrollArea.verticalScrollbar.Select();
                    }
                }
                else
                {
                    // Select scroll bar
                    scrollArea.verticalScrollbar.Select();
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /**
         * Opens this menu for display.
         * <param name="buying">True if the buying menu should be opened, false for selling.</param>
         */
        public void Open(bool buying)
        {
            _buying = buying;
            _inputStateCache = Merchant.Customer.PlayerController.CacheInputActionsState();
            Merchant.Customer.PlayerController.InputActions.Disable();
            Merchant.Customer.PlayerController.InputActions.UI.Enable();
            Merchant.Customer.PlayerController.InputActions.UI.CloseMenu.Disable();
            Merchant.Customer.PlayerController.InputActions.UI.Back.performed += Input_Back;
            gameObject.SetActive(true);
            Cursor.visible = true;
            Refresh();
        }

        /**
         * Closes this menu.
         */
        public void Close()
        {
            Merchant.Customer.PlayerController.InputActions.UI.Back.performed -= Input_Back;
            Merchant.Customer.PlayerController.InputActions.UI.CloseMenu.Enable();
            Merchant.Customer.PlayerController.RestoreInputActionsState(_inputStateCache);
            gameObject.SetActive(false);
            Cursor.visible = false;
            Merchant.Customer.HUD.DialogueDisplay.Resume();
        }

        private void Input_Back(InputAction.CallbackContext context)
        {
            Close();
        }

        protected void OnEnable()
        {
            if (null != Merchant.Customer) BindCustomerEvents();
            if (null != Merchant) BindMerchantEvents();
        }

        protected void OnDisable()
        {
            if (null != Merchant.Customer) UnbindCustomerEvents();
            if (null != Merchant) UnbindMerchantEvents();
        }

        private void BindCustomerEvents()
        {
            Merchant.Customer.OnWalletChanged += HandleWalletChange;
            if (!_buying)
            {
                Merchant.Customer.Inventory.OnEntryAdded += HandleInventoryChange;
                Merchant.Customer.Inventory.OnEntryModified += HandleInventoryChange;
                Merchant.Customer.Inventory.OnEntryRemoved += HandleInventoryChange;
            }
        }

        private void BindMerchantEvents()
        {
            Merchant.Inventory.OnEntryAdded += HandleInventoryChange;
            Merchant.Inventory.OnEntryModified += HandleInventoryChange;
            Merchant.Inventory.OnEntryRemoved += HandleInventoryChange;
        }

        private void UnbindCustomerEvents()
        {
            Merchant.Customer.OnWalletChanged -= HandleWalletChange;
            if (!_buying)
            {
                Merchant.Customer.Inventory.OnEntryAdded -= HandleInventoryChange;
                Merchant.Customer.Inventory.OnEntryModified -= HandleInventoryChange;
                Merchant.Customer.Inventory.OnEntryRemoved -= HandleInventoryChange;
            }
        }

        private void UnbindMerchantEvents()
        {
            Merchant.Inventory.OnEntryAdded -= HandleInventoryChange;
            Merchant.Inventory.OnEntryModified -= HandleInventoryChange;
            Merchant.Inventory.OnEntryRemoved -= HandleInventoryChange;
        }

        private void HandleInventoryChange(InventoryEntry entry)
        {
            Refresh();
        }

        private void HandleWalletChange(int oldValue, int newValue)
        {
            UpdateWalletDisplay(oldValue, newValue);
            Refresh();
        }

        private void UpdateWalletDisplay(int oldValue, int newValue)
        {
            if (walletText == null) return;
            
            // TODO: Play an animation of increasing or decreasing money
            
            string newString = newValue.ToString();
            int digits = newString.Length;
            int extraSpaces = minCurrencyDigits > digits ? minCurrencyDigits - digits : 0;
            char[] buffer = new char [digits + extraSpaces + 1];
            
            // Compute the string
            buffer[0] = '$';
            int i = 1;
            while (i <= extraSpaces)
            {
                buffer[i++] = ' ';
            }
            for (int d = 0; d < digits; d++, i++)
            {
                buffer[i] = newString[d];
            }

            walletText.text = new string(buffer);
        }
    }
}