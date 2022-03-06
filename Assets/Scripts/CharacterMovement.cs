using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class CharacterMovement : MonoBehaviour
    {
        // Speed at which the character moves
        [SerializeField] private float moveSpeed = 5;
        // Magnitude of force applied to character when jumping
        [SerializeField] private float jumpForce = 12;
        // Magnitude of force applied to character when double jumping
        [SerializeField] private float doubleJumpForce = 12;

        // This object's rigidbody
        private Rigidbody2D _rigidbody;

        // Number of double jumps the player can do
        private int _maxJumps = 2;
        // Whether the player has already spent their double jump
        private int _numJumps;
        // Whether the player has unlocked the ability to double jump
        private bool _doubleJumpEnabled = true;

        // Whether the character is moving left this frame
        private bool _movingLeft = false;
        // Whether the character is moving right this frame
        private bool _movingRight = false;
        // Whether the character is moving up this frame
        private bool _movingUp = false;
        // Whether the character is moving down this frame
        private bool _movingDown = false;

        // Whether the player is currently touching the ground
        private bool _isGrounded = false;

        // The distance from the character's position to the bottom of the sprite. Used for grounded testing
        private float _yDist;

        public bool IsGrounded => _isGrounded;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _yDist = GetComponent<Collider2D>().bounds.extents.y + 0.1f;
        }

        public void FixedUpdate()
        {
            // Move based on the set movement flags

            if(_movingLeft)
            {
                transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
                _movingLeft = false;
            }

            if(_movingRight)
            {
                transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
                _movingRight = false;
            }

            if (_movingUp)
            {
                transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
                _movingUp = false;
            }

            if (_movingDown)
            {
                transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);
                _movingDown = false;
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // When a character enters a collision, check whether it has landed on the ground
            checkGrounded();
        }

        // When a character exits a collision, check whether it has left the ground
        void OnCollisionExit2D(Collision2D collision)
        {
            // When a character exits a collision, check whether it has left the ground
            checkGrounded();
        }

        // Translate the character left based on their move speed
        public void MoveLeft()
        {
            _movingLeft = true;
        }

        // Translate the character right based on their move speed
        public void MoveRight()
        {
            _movingRight = true;
        }

        // Translate the character up based on their move speed
        public void MoveUp()
        {
            _movingUp = true;
        }

        // Translate the character down based on their move speed
        public void MoveDown()
        {
            _movingDown = true;
        }

        // Check if a jump is allowed, and if so, jump
        public void StartJump()
        {
            // If the player is on the ground, a jump is permitted
            if (_isGrounded)
            {
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            // If the player has double jumps remaining, a jump is permitted
            else if (_doubleJumpEnabled && _numJumps > 0)
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0);
                _rigidbody.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);
                _numJumps--;
            }
        }

        // Checks if the player is grounded, and updates isGrounded value accordingly
        void checkGrounded()
        {
            _isGrounded = Physics2D.Raycast(transform.position, Vector2.down, _yDist);

            if (_isGrounded)
            {
                _numJumps = _maxJumps;
            }
        }
    }
}
