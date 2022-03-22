using UnityEngine;

namespace BuildABot
{
    
    /**
     * The core data and logic associated with the Player character.
     */
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerInput))]
    public class Player : MonoBehaviour
    {
        
        // TODO: Create a character base class? Coordinate with Enemy and NPC development

        [Tooltip("The attributes available to this player.")]
        [SerializeField] private CharacterAttributeSet attributes;

        /** The attribute set used by this character. */
        public CharacterAttributeSet Attributes => attributes;

        /** The player movement component used by this player. */
        private PlayerMovement _playerMovement;
        /** The player input component used by this player. */
        private PlayerInput _playerInput;

        /** The player movement controller used by this player. */
        public PlayerMovement PlayerMovement => _playerMovement;
        /** The player input component used by this player. */
        public PlayerInput PlayerInput => _playerInput;
        
#region Follow Mouse Debug tool

        [Header("Debug")]
        [SerializeField] private bool useFollowMouseTool;

        private Vector2 _target;
        private Camera _mainCamera;
        
#endregion

        protected void Awake()
        {
            attributes.Initialize();
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