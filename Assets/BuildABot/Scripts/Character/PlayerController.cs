using UnityEngine;
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
            InputActions.Player.Enable();
        }

        protected void OnEnable()
        {
            // Bind player inputs
            
            InputActions.Player.Jump.performed += Player_OnJump;
            
            InputActions.Player.LightAttack.performed += Player_OnLightAttack;
            InputActions.Player.HeavyAttack.performed += Player_OnHeavyAttack;
            InputActions.Player.Guard.performed += Player_OnGuard;
            
            InputActions.Player.OpenMenu.performed += Player_OnOpenMenu;
            InputActions.Player.OpenInventory.performed += Player_OnOpenInventory;
            
            // Bind UI inputs

            InputActions.UI.CloseMenu.performed += UI_OnCloseMenu;

            // Bind dialogue and inputs from their respective classes
        }

        protected void OnDisable()
        {
            // Unbind player inputs
            
            InputActions.Player.Jump.performed -= Player_OnJump;
            
            InputActions.Player.LightAttack.performed -= Player_OnLightAttack;
            InputActions.Player.HeavyAttack.performed -= Player_OnHeavyAttack;
            InputActions.Player.Guard.performed -= Player_OnGuard;
            
            InputActions.Player.OpenMenu.performed -= Player_OnOpenMenu;
            InputActions.Player.OpenInventory.performed -= Player_OnOpenInventory;
            
            // Unbind UI inputs

            InputActions.UI.CloseMenu.performed -= UI_OnCloseMenu;

            // Unbind dialogue and inputs from their respective classes
        }

        protected void Update()
        {

            if (InputActions.Player.enabled)
            {
                Vector2 move = InputActions.Player.Move.ReadValue<Vector2>();
                _player.CharacterMovement.MoveHorizontal(move.x);
                _player.CharacterMovement.MoveVertical(move.y); // For flying mode only

                //Vector2 cameraOffset = _inputActions.Player.Camera.ReadValue<Vector2>();
            }
            
            // Legacy
            /*if (GameInputEnabled)
            {
                _player.CharacterMovement.MoveHorizontal(Input.GetAxisRaw("Horizontal"));
                _player.CharacterMovement.MoveVertical(Input.GetAxisRaw("Vertical")); // For flying mode only
            
                // If jump is pressed, FixedUpdate will determine if they are allowed to jump on this frame
                if (Input.GetButtonDown("Jump"))
                {
                    _player.CharacterMovement.Jump();
                }

                if (Input.GetButtonDown("Fire1"))
                {
                    //StartCoroutine(_combatController.Attack());
                    _combatController.DoStoredAttack();
                }
            }

            if (UIInputEnabled)
            {
                if (Input.GetButtonDown("Open Menu"))
                {
                    _player.ToggleMenu();
                }
            }*/
        }
        
        #region Input Handlers
        
        #region Player

        private void Player_OnJump(InputAction.CallbackContext context)
        {
            _player.CharacterMovement.Jump();
        }

        private void Player_OnLightAttack(InputAction.CallbackContext context)
        {
            _combatController.DoStoredAttack();
        }

        private void Player_OnHeavyAttack(InputAction.CallbackContext context)
        {
            Debug.Log("Heavy attack");
        }

        private void Player_OnGuard(InputAction.CallbackContext context)
        {
            Debug.Log("Guard");
        }

        private void Player_OnOpenMenu(InputAction.CallbackContext context)
        {
            _player.ToggleMenu();
        }

        private void Player_OnOpenInventory(InputAction.CallbackContext context)
        {
            _player.ToggleMenu();
            // TODO: Open inventory directly
        }
        
        #endregion
        
        #region UI
        
        private void UI_OnCloseMenu(InputAction.CallbackContext context)
        {
            _player.ToggleMenu();
        }
        
        #endregion
        
        #endregion
    }
}