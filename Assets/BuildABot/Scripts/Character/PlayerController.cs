using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BuildABot
{
    
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        private Player _player;
        private CombatController _combatController;

        /** The input actions used by this controller. */
        private PlayerInputActions _inputActions;
        /** The player input component used by this object. */
        private PlayerInput _playerInput;
        /** The camera controller component used by this object */
        private CameraController _cameraController;

        /** The input actions of this program mapped to a key identifier. */
        private readonly Dictionary<string, InputAction> _inputActionLookup = new Dictionary<string, InputAction>();

        /** The input events bound to this controller indexed by their associated action. */
        private readonly Dictionary<InputAction, UnityEvent> _inputEventLookup = new Dictionary<InputAction, UnityEvent>();

        /**
         * A state cached used to store information about the enabled action maps in an input actions asset.
         */
        public class InputActionsStateCache
        {
            /** The cached states. */
            private readonly Dictionary<InputActionMap, bool> _inputActionsStateCache;

            /**
             * Constructs a new cache.
             * <param name="asset">The asset to generate the cache from.</param>
             */
            public InputActionsStateCache(InputActionAsset asset)
            {
                _inputActionsStateCache = new Dictionary<InputActionMap, bool>();
                
                foreach (InputActionMap am in asset.actionMaps)
                {
                    _inputActionsStateCache.Add(am, am.enabled);
                }
            }

            /**
             * Applies this cache to the provided asset. This will only be successful if the asset is the same one this
             * cache was built from.
             * <param name="asset">The asset to apply this cache to.</param>
             */
            public void ApplyToAsset(InputActionAsset asset)
            {
                foreach (InputActionMap am in asset.actionMaps)
                {
                    if (_inputActionsStateCache.TryGetValue(am, out bool enable) && enable) am.Enable();
                    else am.Disable();
                }
            }
        }

        /** The input actions used for this player controller. */
        public PlayerInputActions InputActions
        {
            get { return _inputActions ??= new PlayerInputActions(); } // Initialize if null
        }

        /** The player input component used by this controller. */
        public PlayerInput PlayerInput => _playerInput;

        protected void Awake()
        {
            _player = GetComponent<Player>();
            _combatController = GetComponent<CombatController>();
            _playerInput = GetComponent<PlayerInput>();
            _cameraController = GetComponent<CameraController>();
            InputActions.Player.Enable();
            
            // Gather input lookups
            foreach (InputActionMap actionMap in InputActions.asset.actionMaps)
            {
                foreach (InputAction action in actionMap)
                {
                    if (action != null)
                    {
                        _inputActionLookup.Add($"{{INPUT:{actionMap.name}:{action.name}}}", action);
                        _inputEventLookup.Add(action, new UnityEvent());
                        action.performed += HandleInputAction;
                    }
                }
            }
        }

        private void HandleInputAction(InputAction.CallbackContext callback)
        {
            if (_inputEventLookup.TryGetValue(callback.action, out UnityEvent e))
            {
                e.Invoke();
            }
        }

        protected void OnEnable()
        {
            // Bind player inputs
            
            InputActions.Player.Jump.performed += Player_OnJump;
            
            InputActions.Player.LightAttack.performed += Player_OnLightAttack;
            InputActions.Player.HeavyAttack.performed += Player_OnHeavyAttack;
            InputActions.Player.AoeAttack.performed += Player_OnAoeAttack;
            InputActions.Player.ProjectileAttack.performed += Player_OnProjectileAttack;
            
            InputActions.Player.OpenMenu.performed += Player_OnOpenMenu;
            InputActions.Player.OpenInventory.performed += Player_OnOpenInventory;
            
            // Bind UI inputs

            InputActions.UI.CloseMenu.performed += UI_OnCloseMenu;
        }

        protected void OnDisable()
        {
            // Unbind player inputs
            
            InputActions.Player.Jump.performed -= Player_OnJump;
            
            InputActions.Player.LightAttack.performed -= Player_OnLightAttack;
            InputActions.Player.HeavyAttack.performed -= Player_OnHeavyAttack;
            InputActions.Player.AoeAttack.performed -= Player_OnAoeAttack;
            InputActions.Player.ProjectileAttack.performed -= Player_OnProjectileAttack;
            
            InputActions.Player.OpenMenu.performed -= Player_OnOpenMenu;
            InputActions.Player.OpenInventory.performed -= Player_OnOpenInventory;
            
            // Unbind UI inputs

            InputActions.UI.CloseMenu.performed -= UI_OnCloseMenu;
        }

        private void OnDestroy()
        {
            // Clear input lookups
            foreach (var entry in _inputEventLookup)
            {
                if (entry.Key != null)
                {
                    entry.Key.performed -= HandleInputAction;
                }
            }
            _inputActionLookup.Clear();
            _inputEventLookup.Clear();
        }

        protected void Update()
        {

            if (InputActions.Player.enabled)
            {
                Vector2 move = InputActions.Player.Move.ReadValue<Vector2>();
                _player.CharacterMovement.MoveHorizontal(move.x);
                _player.CharacterMovement.MoveVertical(move.y); // For flying mode only

                Vector2 cameraOffset = InputActions.Player.Camera.ReadValue<Vector2>();
                _cameraController.CameraLook(cameraOffset);

                float zoomOut = InputActions.Player.ZoomOut.ReadValue<float>();
                _cameraController.ZoomOut(zoomOut != 0);
            }
        }

        /**
         * Binds the provided action to the input action associated with the provided input path.
         * Input paths are in the form {INPUT:ActionMap:Action}.
         * <param name="inputPath">The input path to bind to.</param>
         * <param name="action">The action to bind to the input event.</param>
         * <returns>True if the operation was successful, false otherwise.</returns>
         */
        public bool BindInputEvent(string inputPath, UnityAction action)
        {
            if (_inputActionLookup.TryGetValue(inputPath, out InputAction inputAction))
            {
                if (_inputEventLookup.TryGetValue(inputAction, out UnityEvent e))
                {
                    e.AddListener(action);
                    return true;
                }
            }
            return false;
        }

        /**
         * Unbinds the provided action from the input action associated with the provided input path.
         * Input paths are in the form {INPUT:ActionMap:Action}.
         * <param name="inputPath">The input path to bind to.</param>
         * <param name="action">The action to unbind from the input event.</param>
         * <returns>True if the input path was valid, false otherwise.</returns>
         */
        public bool UnbindInputEvent(string inputPath, UnityAction action)
        {
            if (_inputActionLookup.TryGetValue(inputPath, out InputAction inputAction))
            {
                if (_inputEventLookup.TryGetValue(inputAction, out UnityEvent e))
                {
                    e.RemoveListener(action);
                    return true;
                }
            }
            return false;
        }

        /**
         * Generates a cache of the current enabled state of all input maps in the input actions data. This state can
         * be applied by calling RestoreInputActionState(InputActionsStateCache).
         * <returns>The cached input actions state.</returns>
         */
        public InputActionsStateCache CacheInputActionsState()
        {
            return new InputActionsStateCache(InputActions.asset);
        }

        /**
         * Restores the enabled state for all input maps in the input actions data. This state is saved using the
         * CacheInputActionsState() method.
         * <param name="state">The cached state to use when restoring the input actions.</param>
         */
        public void RestoreInputActionsState(InputActionsStateCache state)
        {
            state.ApplyToAsset(InputActions.asset);
        }
        
        #region Input Handlers
        
        #region Player

        private void Player_OnJump(InputAction.CallbackContext context)
        {
            _player.CharacterMovement.Jump();
        }

        private void Player_OnLightAttack(InputAction.CallbackContext context)
        {
            if (_combatController.DoLightMeleeAttack())
            {
                ComputerPartInstance item = _player.GetItemEquippedToSlot(EComputerPartSlot.Mouse);
                if (item != null)
                {
                    item.ApplyDamage(1);
                }
            }
        }

        private void Player_OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (_combatController.DoHeavyMeleeAttack())
            {
                ComputerPartInstance item = _player.GetItemEquippedToSlot(EComputerPartSlot.Keyboard);
                if (item != null)
                {
                    item.ApplyDamage(1);
                }
            }
        }

        private void Player_OnAoeAttack(InputAction.CallbackContext context)
        {
            _combatController.DoAreaOfEffectAttack();
        }

        private void Player_OnProjectileAttack(InputAction.CallbackContext context)
        {
            _combatController.DoProjectileAttack();
        }

        private void Player_OnOpenMenu(InputAction.CallbackContext context)
        {
            _player.OpenMenu();
        }

        private void Player_OnOpenInventory(InputAction.CallbackContext context)
        {
            _player.OpenMenu();
            _player.MainMenu.OpenInventory();
        }
        
        #endregion
        
        #region UI
        
        private void UI_OnCloseMenu(InputAction.CallbackContext context)
        {
            _player.CloseMenu();
        }
        
        #endregion
        
        #endregion
    }
}