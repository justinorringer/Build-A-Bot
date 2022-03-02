using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    // Speed at which the character moves
    [SerializeField] private float moveSpeed = 5;
    // Magnitude of force applied to character when jumping
    [SerializeField] private float jumpForce = 1;

    // THis object's rigidbody
    private Rigidbody2D _rigidbody;

    // Whether the player has pressed space to activate a jump (used for telling physics calculations in FixedUpdate about inputs read in Update)
    private bool _jumping = false;

    // Whether the player has already spent their double jump
    private bool _secondJump = false;
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
        _yDist = GetComponent<Collider2D>().bounds.extents.y;
    }

    // Update is called once per frame
    void Update()
    {
        // If A or left arrow is pressed, move left
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
        }
        // If D or right arrow is pressed, move right
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
        }

        // If space is pressed, FixedUpdate will determine if they are allowed to jump on this frame
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _jumping = true;
        }
    }

    private void FixedUpdate()
    {
        // If jump input was read in Update, determine if a jump is permitted
        if (_jumping)
        {
            // If the player is on the ground, a jump is permitted
            if (_isGrounded)
            {
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            else if(_doubleJumpEnabled && _secondJump)
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0);
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                _secondJump = false;
            }

            _jumping = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        checkGrounded();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        checkGrounded();
    }

    private void checkGrounded()
    {
        _isGrounded = Physics2D.Raycast(transform.position, Vector2.down, _yDist + 0.1f);
        if (_isGrounded)
        {
            _secondJump = true;
        }
    }
}
