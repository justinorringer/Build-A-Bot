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

        /** The text replacement tokens to use for this play session. */
        private Dictionary<string, string> _textReplacementTokens;

        /** The player input controller used by this player. */
        public PlayerController PlayerController => playerController;

        public override CharacterMovement CharacterMovement => playerMovement;

        /** The HUD used by this player. */
        public HUD HUD => hud;

        /** The main menu used by this player. */
        public MainMenu MainMenu => mainMenu;

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
                wallet = value < 0 ? 0 : value;
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
            if (entry is ComputerPartInstance cp)
            {
                // Handle losing all durability
                if (cp.Durability == 0 && cp.MaxDurability != 0)
                {
                    if (entry.Equipped) UnequipItemSlot(cp.ComputerPartItem.PartType);
                    Inventory.RemoveEntry(entry, true);
                }
            }
        }
        
#endregion

        protected override void Awake()
        {
            base.Awake();
            
            Attributes.Initialize();
            Cursor.visible = false;
            SetPaused(false);
            EnableHUD();
            mainMenu.gameObject.SetActive(false);
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
                Debug.Log($"Wallet has {Wallet} coins");
            }
        }

        protected override void Kill()
        {
            onDeath.Invoke();
            // TODO: Play death animation
            FinishGame("Game Over");
        }

        public void FinishGame(string message)
        {
            // TODO: Replace with a more graceful implementation for winning
            SetPaused(true);
            DisableHUD();
            mainMenu.gameObject.SetActive(false);
            StartCoroutine(WaitForGameOver(message));
        }

        private IEnumerator WaitForGameOver(string message)
        {
            gameOverDisplay.Message = message;
            gameOverDisplay.gameObject.SetActive(true);
            yield return new WaitUntil(() => gameOverDisplay.IsFinished);
            // Quit to main menu
            SceneManager.LoadScene("BuildABot/Scenes/StartMenuScene", LoadSceneMode.Single);
        }

        /**
         * Toggles the main menu for this player.
         */
        public void ToggleMenu()
        {
            bool active = !mainMenu.gameObject.activeSelf;
            if (active)
            {
                PlayerController.InputActions.Player.Disable();
                PlayerController.InputActions.UI.Enable();
            }
            else
            {
                PlayerController.InputActions.Player.Enable();
                PlayerController.InputActions.UI.Disable();
            }
            mainMenu.gameObject.SetActive(active);
            Cursor.visible = active;
            hud.gameObject.SetActive(!active);
            SetPaused(active);
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
         * Sets the game paused state.
         * TODO: Move pausing behavior to a global game state controller
         * <param name="paused">true if the game should be paused, false to unpause.</param>
         */
        public void SetPaused(bool paused)
        {
            Time.timeScale = paused ? 0.0f : 1;
            //PlayerController.GameInputEnabled = !paused;
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
         * Show a help menu to the player.
         * <param name="message">The message to display.</param>
         * <param name="title">The title of the alert.</param>
         * <param name="acknowledgeMessage">The text to place in the acknowledgement button. This will use the global help widget default if not provided.</param>
         */
        public void ShowHelpMenu(string message, string title, string acknowledgeMessage = HelpWidget.DefaultAcknowledgeMessage)
        {
            Cursor.visible = true;
            HelpWidget widget = Instantiate(alertPrefab);
            widget.Initialize(this, message, title, acknowledgeMessage);
            SetPaused(true);
            DisableHUD();
        }

        private void SetupStandardTextTokens()
        {
            
            _textReplacementTokens = new Dictionary<string, string>
            {
                {"{PLAYER_NAME}", "BIPY"}
            };
            
            // Gather input mapping tokens
            foreach (var actionMap in PlayerController.InputActions.asset.actionMaps)
            {
                foreach (var action in actionMap)
                {
                    if (action != null)
                    {
                        var bindingIndex = action.GetBindingIndex(PlayerController.PlayerInput.currentControlScheme);
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

    }
}