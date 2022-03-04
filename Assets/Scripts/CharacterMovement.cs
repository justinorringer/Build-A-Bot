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

        private bool _jumping = false;

        // Number of double jumps the player can do
        private int _maxJumps = 2;
        // Whether the player has already spent their double jump
        private int _numJumps;
        // Whether the player has unlocked the ability to double jump
        private bool _doubleJumpEnabled = true;

        // Whether the player is currently touching the ground
        private bool _isGrounded = false;

        // The distance from the character's position to the bottom of the sprite. Used for grounded testing
        private float _yDist;

        public bool IsGrounded => _isGrounded;

        // Start is called before the first frame update
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _yDist = GetComponent<Collider2D>().bounds.extents.y + 0.1f;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            checkGrounded();
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            checkGrounded();
        }

        public void MoveLeft()
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }

        public void MoveRight()
        {
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
        }

        public void MoveUp()
        {
            transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
        }
        
        public void MoveDown()
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);
        }

        public void StartJump()
        {
            // If the player is on the ground, a jump is permitted
            if (_isGrounded)
            {
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            else if (_doubleJumpEnabled && _numJumps > 0)
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0);
                _rigidbody.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);
                _numJumps--;
            }
            //_jumping = true;
        }

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
