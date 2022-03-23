using UnityEngine;

namespace BuildABot
{
    
    /**
     * The core data and logic associated with the Player character.
     */
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerInput))]
    public class Player : Character
    {

        /** The player movement component used by this player. */
        private PlayerMovement _playerMovement;
        /** The player input component used by this player. */
        private PlayerInput _playerInput;

        /** The player input component used by this player. */
        public PlayerInput PlayerInput => _playerInput;

        public override CharacterMovement CharacterMovement => _playerMovement;

#region Follow Mouse Debug tool

        [Header("Debug")]
        [SerializeField] private bool useFollowMouseTool;

        private Vector2 _target;
        private Camera _mainCamera;
        
#endregion

        protected override void Awake()
        {
            base.Awake();
            Attributes.Initialize();
        }
        
        
        protected void Start()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _playerInput = GetComponent<PlayerInput>();

            if (useFollowMouseTool)
            {
                _playerInput.InputEnabled = false;
                _playerMovement.ChangeMovementMode(ECharacterMovementMode.Flying);
                _mainCamera = Camera.main;
            }
        }

        protected void Update()
        {
            if (useFollowMouseTool)
            {
                _playerMovement.MoveToPosition(_target);
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = _mainCamera.nearClipPlane;
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
                _target = new Vector2(worldPos.x, worldPos.y);
            }
        }

    }
}