using System.Collections;
using UnityEngine;

namespace BuildABot
{

    /**
     * The movement modes available to characters.
     */
    public enum ECharacterMovementMode
    {
        Walking,
        Flying
    }
    
    /**
     * The base class used for all types of character movement.
     * Derived classes need to provide a reference to its source attribute set data by overriding
     * the SourceAttributes getter property.
     */
    public abstract class CharacterMovement : MonoBehaviour
    {

        [Tooltip("The movement mode used by this character.")]
        [SerializeField] private ECharacterMovementMode movementMode = ECharacterMovementMode.Walking;
        
        /** The attribute set driving this controller. */
        protected abstract CharacterAttributeSet SourceAttributes { get; }

        /** The movement speed of this character. */
        public virtual float MovementSpeed => SourceAttributes.MovementSpeed.CurrentValue;
        /** The base jump force of this character. */
        public virtual float JumpForce => SourceAttributes.JumpForce.CurrentValue;
        /** The falloff applied to jump force after each multi-jump. */
        public virtual float JumpForceFalloff => SourceAttributes.JumpForceFalloff.CurrentValue;
        
        /** The maximum jumps this character can perform. This is 0 if the character is not in Waling mode.*/
        public virtual int MaxJumps => IsWalking ? SourceAttributes.MaxJumpCount.CurrentValue : 0;

        /** The movement mode this character is currently using. */
        public ECharacterMovementMode MovementMode => movementMode;

        /** Is the character in walking mode? */
        public bool IsWalking => MovementMode == ECharacterMovementMode.Walking;
        /** Is the character in flying mode? */
        public bool IsFlying => MovementMode == ECharacterMovementMode.Flying;

        /** This object's rigidbody */
        private Rigidbody2D _rigidbody;

        /** This object's collider 2d */
        private Collider2D _collider;

        /** The sprite renderer used by this object. */
        private SpriteRenderer _sprite;

        /** How many jumps has the character attempted? */
        private int _jumpCount;
        /** The current jump force available to this character. */
        private float _jumpForce;

        /** The current horizontal movement rate. */
        private float _horizontalMovementRate;
        /** The current vertical movement rate. */
        private float _verticalMovementRate;

        /** The extents of the character. Equal to the half bounds. */
        private Vector2 _extents;

        /** Unused reference for velocity dampening. */
        private Vector2 _tempVelocity = Vector2.zero;

        /** Whether the player is currently touching the ground. */
        private bool _isGrounded;

        /** The root foot position of the character used for the ground check. */
        private Vector2 RootPosition => _rigidbody.position + (Vector2.down * _extents.y);

        /** Is the character grounded? */
        public bool IsGrounded => _isGrounded;

        /** The normalized movement direction of this character. */
        public Vector2 MovementDirection => _rigidbody.velocity.normalized;

        /** Which direction the character is currently facing */
        private Vector2 _facing = Vector2.right;

        /** Which direction is the character currently facing? */
        public Vector2 Facing => _facing;

        /** The original assigned gravity scale of the rigidbody */
        private float _originalGravity;

        /** Gravity scale multiplier of the jump during the upward arc */
        [SerializeField] private float upArcGravity;
        /** Gravity scale multiplier of the jump during the peak */
        [SerializeField] private float jumpPeakGravity;
        /** Duration of the gravity scale at the peak */
        [SerializeField] private float jumpPeakDuration;
        /** Gravity scale multiplier of the jump during the downward arc */
        [SerializeField] private float downArcGravity;

        /** Time it takes for the player to reach their max speed when they start moving */
        [SerializeField] private float accelerationTime = 0.05f;
        /** Time it takes for the player to reach zero speed when they stop moving */
        [SerializeField] private float decelerationTime = 0.05f;

        protected virtual void Awake()
        {
            
        }

        protected virtual void Start()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            
            Bounds bounds = _collider.bounds;
            _extents = new Vector2(bounds.extents.x, bounds.extents.y);

            if (IsFlying) _rigidbody.gravityScale = 0;
            else _rigidbody.gravityScale = 2.5f;
            
            CheckGrounded();
            _jumpCount = 0;
            _jumpForce = JumpForce;

            _originalGravity = _rigidbody.gravityScale;
        }

        protected void FixedUpdate()
        {
            // Check grounded state each frame
            CheckGrounded();

            float movementRate = MovementSpeed * Time.fixedDeltaTime * 10.0f;

            // Move using velocity based on the cached movement rates
            Vector2 targetVelocity;
            switch (MovementMode)
            {
                case ECharacterMovementMode.Walking:
                    Vector2 velocity = _rigidbody.velocity;
                    targetVelocity = new Vector2(_horizontalMovementRate * movementRate, velocity.y);
                    break;
                case ECharacterMovementMode.Flying:
                    targetVelocity = new Vector2(_horizontalMovementRate, _verticalMovementRate) * movementRate;
                    break;
                default:
                    targetVelocity = _rigidbody.velocity;
                    break;
            }
            float dampTime = _rigidbody.velocity.magnitude < targetVelocity.magnitude ? accelerationTime : decelerationTime;
            _rigidbody.velocity = Vector2.SmoothDamp(_rigidbody.velocity, targetVelocity, ref _tempVelocity, dampTime);

            // Update the direction the character is facing if it has changed
            Vector2 dir = targetVelocity.normalized;
            if (dir.x != 0.0f)
            {
                bool isRight = dir.x > 0.0f;
                _facing = isRight ? Vector2.right : Vector2.left;
                _sprite.flipX = !isRight;
            }
        }

        /**
         * Used to apply horizontal movement using this controller. A negative value moves left, positive values move right.
         * <param name="amount">The directional input to apply. This should be between -1 and 1.</param>
         */
        public void MoveHorizontal(float amount)
        {
            _horizontalMovementRate = amount;
        }

        /**
         * Used to apply vertical movement using this controller. A negative value moves down, positive values move up.
         * <param name="amount">The directional input to apply. This should be between -1 and 1.</param>
         */
        public void MoveVertical(float amount)
        {
            if (IsFlying) _verticalMovementRate = amount;
        }

        public void MoveToPosition(Vector2 position)
        {
            Vector2 delta = (position - _rigidbody.position);
            if (delta.sqrMagnitude > 1.0f) delta.Normalize();
            _horizontalMovementRate = delta.x;
            _verticalMovementRate = delta.y;
        }

        /**
         * Changes the movement mode of this controller to the provided mode.
         * <param name="mode">The new movement mode to use.</param>
         */
        public void ChangeMovementMode(ECharacterMovementMode mode)
        {
            movementMode = mode;
            if (IsFlying) _rigidbody.gravityScale = 0;
            else _rigidbody.gravityScale = 2.5f;
        }

        /**
         * Makes the character jump if the character is grounded or has available multi-jumps.
         * This will only work for characters with a Walking movement mode.
         */
        public void Jump()
        {
            // If the player has jumps available and is in a grounded movement mode, jump
            if (_jumpCount < MaxJumps)
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0); // Clear any existing vertical velocity
                _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse); // Add the impulse
                _jumpCount++; // Update the jump count
                _jumpForce *= JumpForceFalloff; // Apply the force falloff
                StopCoroutine(JumpPhysics()); // Stop jump coroutine of any previous jumps
                StartCoroutine(JumpPhysics()); // Start a new jump coroutine
            }
        }

        /**
         * Handles any changes to jump physics during the jump
         */
        private IEnumerator JumpPhysics()
        {
            // Make sure gravity scale is set to original value in case it had been changed by a different jump
            _rigidbody.gravityScale = _originalGravity * upArcGravity;

            // The period of the time before the top of the arc (the top of the arc is represented by the moment y velocity = 0)
            while (_rigidbody.velocity.y > 0)
            {
                yield return null;
            }

            // Change the gravity scale at the peak of the jump for the specified number of seconds
            _rigidbody.gravityScale = _originalGravity * jumpPeakGravity;
            yield return new WaitForSeconds(jumpPeakDuration);

            // Change the gravity scale on the downaward arc of the jump
            _rigidbody.gravityScale = _originalGravity * downArcGravity;

            while(_rigidbody.velocity.y < 0)
            {
                yield return null;
            }

            // Reset gravity scale to original value
            _rigidbody.gravityScale = _originalGravity;
        }
        
        private void OnCollisionStay2D(Collision2D other)
        {
            if (_isGrounded && _jumpCount > 0 && MovementDirection.y <= 0f)
            {
                _jumpCount = 0;
                _jumpForce = JumpForce;
            }
        }

        /**
         * Checks if the player is grounded, and updates isGrounded value accordingly.
         */
        private void CheckGrounded()
        {
            // Note: Character can only be grounded if in walking mode
            _isGrounded = Physics2D.BoxCast(RootPosition, new Vector2(_extents.x * 2, 0.1f),
                0, Vector2.down, 0.01f, 
                Physics2D.AllLayers & ~LayerMask.GetMask("Player")) && IsWalking;
        }
    }
}
