using UnityEngine;

namespace BuildABot
{
    public class PlayerInput : MonoBehaviour
    {
        private PlayerMovement _controller;
        private CombatController _combatController;

        public bool InputEnabled { get; set; } = true;
        
        // Start is called before the first frame update
        void Start()
        {
            _controller = GetComponent<PlayerMovement>();
            _combatController = GetComponent<CombatController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (InputEnabled)
            {
                _controller.MoveHorizontal(Input.GetAxisRaw("Horizontal"));
                _controller.MoveVertical(Input.GetAxisRaw("Vertical")); // For flying mode only
            
                // If jump is pressed, FixedUpdate will determine if they are allowed to jump on this frame
                if (Input.GetButtonDown("Jump"))
                {
                    _controller.Jump();
                }

                if (Input.GetButtonDown("Fire1"))
                {
                    //StartCoroutine(_combatController.Attack());
                    _combatController.DoStoredAttack();
                }
            }
        }
        
        
    }
}