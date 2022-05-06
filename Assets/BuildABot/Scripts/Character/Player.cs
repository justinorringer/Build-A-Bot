using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;

namespace BuildABot
{
    
    /**
     * The core data and logic associated with the Player character.
     */
    public class Player : Character
    {

        [Tooltip("The HUD UI object controlled by this player instance.")]
        [SerializeField] private HUD hud;

        [Tooltip("The main menu UI object controlled by this player instance.")]
        [SerializeField] private MainMenu mainMenu;

        [Tooltip("The prefab to instance when displaying a help message.")]
        [SerializeField] private HelpWidget alertPrefab;

        [Tooltip("The display shown on game over.")]
        [SerializeField] private GameOverDisplay gameOverDisplay;

        [Tooltip("The player movement component used by this player.")]
        [SerializeField] private PlayerMovement playerMovement;
        [Tooltip("The player input controller used by this player.")]
        [SerializeField] private PlayerController playerController;

        /** The index of this player. */
        private int _playerIndex = -1;

        /** The text replacement tokens to use for this play session. */
        private Dictionary<string, string> _textReplacementTokens;

        /** Can the menu be toggled? */
        private bool _canToggleMenu = true;

        /** The player input controller used by this player. */
        public PlayerController PlayerController => playerController;

        public override CharacterMovement CharacterMovement => playerMovement;

        /** The index of this player in the game. Based on join order. */
        public int PlayerIndex => _playerIndex;

        /** The HUD used by this player. */
        public HUD HUD => hud;

        /** The main menu used by this player. */
        public MainMenu MainMenu => mainMenu;

        private int _attackSpeedHash;

#region Item and Equipment Handling

        /** The amount of currency owned by this player. */
        [HideInInspector]
        [SerializeField] private int wallet;

        /** The amount of currency held by this player. */
        public int Wallet
        {
            get => wallet;
            set
            {
                int cache = wallet;
                int delta = value - wallet;
                wallet = value < 0 ? 0 : value;
                if (delta > 0) GameManager.GameState.TotalMoneyEarned += delta;
                onWalletChanged.Invoke(cache, wallet);
            }
        }

        [Tooltip("An event triggered whenever this player's wallet is changed. Subscribers will receive the old and new values.")]
        [SerializeField] private UnityEvent<int, int> onWalletChanged;
        
        /** An event triggered whenever this player's wallet is changed. Subscribers will receive the old and new values. */
        public event UnityAction<int, int> OnWalletChanged
        {
            add => onWalletChanged.AddListener(value);
            remove => onWalletChanged.RemoveListener(value);
        }
        
        /**
         * The data stored in a single equipment slot.
         */
        private struct EquipmentSlotData
        {
            /** The item instance that is equipped. */
            public ComputerPartInstance Item;
            /** The list of effects being applied by the item that must be removed when unequipped. */
            public List<AttributeSet.AppliedEffectHandle> AppliedEffects;
        }

        /** The table of equipment slots and their stored data. */
        private readonly Dictionary<EComputerPartSlot, EquipmentSlotData> _equippedItems =
            new Dictionary<EComputerPartSlot, EquipmentSlotData>();
        
        [Tooltip("An event triggered whenever this player equips an item.")]
        [SerializeField] private UnityEvent<ComputerPartInstance> onItemEquipped;
        
        [Tooltip("An event triggered whenever this player unequips an item.")]
        [SerializeField] private UnityEvent<ComputerPartInstance> onItemUnequipped;

        /** The pre-cached hash value for the HasMouse animation flag. */
        private static readonly int HasMouse = Animator.StringToHash("HasMouse");

        /** An event triggered whenever this player equips an item. */
        public event UnityAction<ComputerPartInstance> OnItemEquipped
        {
            add => onItemEquipped.AddListener(value);
            remove => onItemEquipped.RemoveListener(value);
        }
        
        /** An event triggered whenever this player unequips an item. */
        public event UnityAction<ComputerPartInstance> OnItemUnequipped
        {
            add => onItemUnequipped.AddListener(value);
            remove => onItemUnequipped.RemoveListener(value);
        }

        /**
         * Gets the computer part equipped to the specified slot. If nothing is equipped, null is returned.
         * <param name="slot">The type of item slot to check.</param>
         * <returns>The equipped item or null if nothing is equipped.</returns>
         */
        public ComputerPartInstance GetItemEquippedToSlot(EComputerPartSlot slot)
        {
            if (_equippedItems.TryGetValue(slot, out EquipmentSlotData data))
            {
                return data.Item;
            }
            return null;
        }

        /**
         * Equips the provided item to the player. If an item is already equipped to it's slot, it will be unequipped.
         * <param name="item">The item to equip.</param>
         */
        public void EquipItem(ComputerPartInstance item)
        {
            ComputerPartItem baseItem = item.Item as ComputerPartItem;
            Debug.Assert(baseItem != null, 
                "The base item of a ComputerPartInstance should never be null or a non-ComputerPartItem instance.");
            EComputerPartSlot slot = baseItem.PartType;
            
            // Unequip the slots previous item if one exists
            if (_equippedItems.TryGetValue(slot, out _)) UnequipItemSlot(slot);

            switch (slot)
            {
                case EComputerPartSlot.Mouse:
                    CombatController.LightAttack = baseItem.Attack as MeleeAttackData;
                    CharacterMovement.Animator.SetBool(HasMouse, true);
                    break;
                case EComputerPartSlot.Keyboard:
                    CombatController.HeavyAttack = baseItem.Attack as MeleeAttackData;
                    break;
                case EComputerPartSlot.WirelessCard:
                    CombatController.AoeAttack = baseItem.Attack as AoeAttackData;
                    break;
                case EComputerPartSlot.DiskDrive:
                    CombatController.ProjectileAttack = baseItem.Attack as ProjectileAttackData;
                    break;
            }

            // Create the slot data
            EquipmentSlotData data = new EquipmentSlotData()
            {
                Item = item,
                AppliedEffects = new List<AttributeSet.AppliedEffectHandle>()
            };

            // Apply effects
            foreach (EffectInstance effectInstance in baseItem.Effects)
            {
                // Ensure that the effect has the proper target
                if (effectInstance.effect.Target.SelectedType == typeof(CharacterAttributeSet))
                {
                    // Apply the effect
                    AttributeSet.AppliedEffectHandle handle = Attributes.ApplyEffect(effectInstance, this);
                    
                    // When the item is unequipped, any "UntilRemoved" effects will be as well
                    if (effectInstance.effect.DurationMode == EEffectDurationMode.UntilRemoved) data.AppliedEffects.Add(handle);
                }
            }
            
            // Store the slot entry and mark as equipped
            _equippedItems.Add(slot, data);
            item.Equipped = true;
            onItemEquipped.Invoke(item);
        }

        /**
         * Unequips the item from the provided slot if one is currently equipped.
         * <param name="slot">The slot to remove any equipped items from.</param>
         */
        public void UnequipItemSlot(EComputerPartSlot slot)
        {
            if (_equippedItems.TryGetValue(slot, out EquipmentSlotData data))
            {
                switch (slot)
                {
                    case EComputerPartSlot.Mouse:
                        CombatController.LightAttack = null;
                        CharacterMovement.Animator.SetBool(HasMouse, false);
                        break;
                    case EComputerPartSlot.Keyboard:
                        CombatController.HeavyAttack = null;
                        break;
                    case EComputerPartSlot.WirelessCard:
                        CombatController.AoeAttack = null;
                        break;
                    case EComputerPartSlot.DiskDrive:
                        CombatController.ProjectileAttack = null;
                        break;
                }
                
                foreach (AttributeSet.AppliedEffectHandle effect in data.AppliedEffects)
                {
                    Attributes.RemoveEffect(effect);
                }
                // Remove the data and mark as not equipped
                _equippedItems.Remove(slot);
                data.Item.Equipped = false;
                onItemEquipped.Invoke(data.Item);
            }
        }

        private void HandleNewItem(InventoryEntry entry)
        {
            // Handle auto-equipping new items
            if (entry is ComputerPartInstance cp)
            {
                if (GetItemEquippedToSlot(cp.ComputerPartItem.PartType) == null)
                {
                    EquipItem(cp);
                }
            }
        }

        private void HandleRemovedItem(InventoryEntry entry)
        {
            if (entry.Equipped && entry is ComputerPartInstance cp)
            {
                UnequipItemSlot(cp.ComputerPartItem.PartType);
            }
        }

        private void HandleModifiedItem(InventoryEntry entry)
        {
            if (entry is ComputerPartInstance cp && Inventory.ContainsEntry(entry))
            {
                // Handle losing all durability
                if (cp.Durability == 0 && cp.MaxDurability != 0)
                {
                    if (entry.Equipped) UnequipItemSlot(cp.ComputerPartItem.PartType);
                    if (null != Inventory.RemoveEntry(entry, true))
                        PushNotification($"Your {cp.Item.DisplayName} broke!");
                }
            }
        }
        
#endregion

        [Tooltip("An event triggered when this object is destroyed.")]
        [SerializeField] private UnityEvent<Player> onPlayerDestroyed;

        /** An event triggered when this object is destroyed. */
        public event UnityAction<Player> OnPlayerDestroyed
        {
            add => onPlayerDestroyed.AddListener(value);
            remove => onPlayerDestroyed.RemoveListener(value);
        }
        
        protected override void Awake()
        {
            if (GameManager.Initialized && GameManager.GetPlayer() != null)
                Destroy(gameObject);
            base.Awake();
            // Register this player
            GameManager.RegisterPlayer(this, index => _playerIndex = index);
            // Initialize the attribute system
            Attributes.Initialize();
            // Set up game and UI state
            Cursor.visible = false;
            GameManager.SetPaused(false);
            EnableHUD();
            mainMenu.gameObject.SetActive(false);
            DontDestroyOnLoad(gameObject);

            _attackSpeedHash = Animator.StringToHash("AttackSpeed");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Inventory.OnEntryAdded += HandleNewItem;
            Inventory.OnEntryModified += HandleModifiedItem;
            Inventory.OnEntryRemoved += HandleRemovedItem;
            if (CombatController != null) CombatController.OnKill += HandleKill;
        }

        protected override void OnDisable()
        {
            if (CombatController != null) CombatController.OnKill -= HandleKill;
            Inventory.OnEntryAdded -= HandleNewItem;
            Inventory.OnEntryModified -= HandleModifiedItem;
            Inventory.OnEntryRemoved -= HandleRemovedItem;
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            onPlayerDestroyed.Invoke(this);
            base.OnDestroy();
        }

        /**
         * Handles killing another character.
         * <param name="attack">The killing attack.</param>
         * <param name="target">The character killed by this character.</param>
         */
        private void HandleKill(AttackData attack, CombatController target)
        {
            if (target.Character is Enemy enemy)
            {
                Wallet += enemy.DroppedCurrency;
                //Debug.Log($"Wallet has {Wallet} coins");
            }
        }

        protected override void Kill()
        {
            StopCooling();
            onDeath.Invoke();
            GameManager.GameState.TotalDeaths++;
            // TODO: Play death animation
            GameManager.GameOver(this);
        }

        /**
         * Opens the main menu for this player.
         */
        public void OpenMenu()
        {
            if (!_canToggleMenu || mainMenu.gameObject.activeSelf) return;
            PlayerController.InputActions.Player.Disable();
            PlayerController.InputActions.UI.Enable();
            mainMenu.gameObject.SetActive(true);
            Cursor.visible = true;
            hud.gameObject.SetActive(false);
            GameManager.SetPaused(true);
        }

        /**
         * Closes the main menu for this player.
         */
        public void CloseMenu()
        {
            if (!_canToggleMenu || !mainMenu.gameObject.activeSelf) return;
            PlayerController.InputActions.Player.Enable();
            PlayerController.InputActions.UI.Disable();
            mainMenu.gameObject.SetActive(false);
            Cursor.visible = false;
            hud.gameObject.SetActive(true);
            GameManager.SetPaused(false);
        }

        public void EnableHUD()
        {
            hud.gameObject.SetActive(true);
        }

        public void DisableHUD()
        {
            hud.gameObject.SetActive(false);
        }

        /**
         * Show a help menu to the player.
         * <param name="message">The message to display.</param>
         */
        public void ShowHelpMenu(string message)
        {
            ShowHelpMenu(message, HelpWidget.DefaultTitle);
        }

        /**
         * Shows an input help display.
         * <param name="message">The message to display.</param>
         * <param name="inputPath">The input action path to wait for to hide the display.</param>
         */
        public void ShowInputHelp(string message, string inputPath)
        {
            HUD.InputHelpWidget.ShowMessage(message, inputPath);
        }

        /**
         * Force dismisses the current input help widget.
         */
        public void DismissInputHelp()
        {
            HUD.InputHelpWidget.HideMessage();
        }

        /**
         * Pushes a notification to the player.
         * <param name="message">The notification message to push.</param>
         */
        public void PushNotification(string message)
        {
            HUD.NotificationDisplay.ShowMessage(PerformStandardTokenReplacement(message));
        }

        /**
         * Show a help menu to the player.
         * <param name="message">The message to display.</param>
         * <param name="title">The title of the alert.</param>
         * <param name="acknowledgeMessage">The text to place in the acknowledgement button. This will use the global help widget default if not provided.</param>
         */
        public void ShowHelpMenu(string message, string title, string acknowledgeMessage = HelpWidget.DefaultAcknowledgeMessage)
        {
            _canToggleMenu = false;
            Cursor.visible = true;
            HelpWidget widget = Instantiate(alertPrefab);
            widget.Initialize(this, message, title, acknowledgeMessage, () => _canToggleMenu = true);
            DisableHUD();
        }

        private void SetupStandardTextTokens()
        {
            
            _textReplacementTokens = new Dictionary<string, string>
            {
                {"{PLAYER_NAME}", "BIPY"}
            };
            
            // Gather input mapping tokens
            foreach (InputActionMap actionMap in PlayerController.InputActions.asset.actionMaps)
            {
                foreach (InputAction action in actionMap)
                {
                    if (action != null)
                    {
                        int bindingIndex = action.GetBindingIndex(PlayerController.PlayerInput.currentControlScheme);
                        if (bindingIndex != -1)
                        {
                            action.GetBindingDisplayString(bindingIndex, out _,
                                out string controlPath, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);

                            bool isGamepad = Gamepad.current != null;
                            string iconsSource;
                            if (isGamepad && PlayerController.PlayerInput.currentControlScheme == "Gamepad")
                            {
                                if (Gamepad.current is DualShockGamepad)
                                {
                                    iconsSource = "PlaystationIcons";
                                }
                                else if (Gamepad.current is XInputController)
                                {
                                    iconsSource = "XboxIcons";
                                }
                                else
                                {
                                    iconsSource = "XboxIcons"; // TODO: Use generic icons
                                }
                            }
                            else
                            {
                                iconsSource = "KeyboardMouseIcons";
                            }
                            string spriteName = controlPath?.Replace('/', '_');
                            
                            if (spriteName != null)
                                _textReplacementTokens.Add($"{{INPUT:{actionMap.name}:{action.name}}}",
                                    $"<sprite=\"{iconsSource}\" name=\"{spriteName}\">");
                        }
                    }
                }
            }
        }

        /**
         * Handles replacing any standard tokens with their displayed value.
         * <param name="source">The source string to replace tokens within.</param>
         */
        public string PerformStandardTokenReplacement(string source)
        {
            SetupStandardTextTokens();
            return source.ReplaceTokens(_textReplacementTokens);
        }

        public void UpdateAttackSpeed()
        {
            playerMovement.Animator.SetFloat(_attackSpeedHash, Attributes.AttackSpeed.CurrentValue);
        }
    }
}