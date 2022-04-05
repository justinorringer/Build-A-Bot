using UnityEngine;

namespace BuildABot
{
    public class PlayerController : MonoBehaviour
    {
        private Player _player;
        private CombatController _combatController;

        public bool GameInputEnabled { get; set; } = true;
        public bool UIInputEnabled { get; set; } = true;
        
        // Start is called before the first frame update
        void Start()
        {
            _player = GetComponent<Player>();
            _combatController = GetComponent<CombatController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (GameInputEnabled)
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
            }
        }
        
        
    }
}