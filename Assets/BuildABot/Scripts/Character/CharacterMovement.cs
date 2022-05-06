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

        [Tooltip("Should the facing direction of this character's sprite be flipped?")]
        [SerializeField] private bool flipSprite;
        
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

        /** The animator used by this object. */
        private Animator _anim;

        /** The animator used by this object. */
        private AudioSource _audio;

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

        /** Unused reference for velocity dampening. */
        private float _tempXVelocity;

        /** Whether the player is currently touching the ground. */
        private bool _isGrounded;

        /** Whether the player is currently walking or flying (depending on movement mode). */
        private bool _inMotion;

        /** The root foot position of the character used for the ground check. */
        private Vector2 RootPosition => _rigidbody.position + (Vector2.down * _extents.y);

        /** Is the character grounded? */
        public bool IsGrounded => _isGrounded;

        /** Is the character grounded? */
        public bool InMotion => _inMotion;

        /** The normalized movement direction of this character. */
        public Vector2 MovementDirection => _rigidbody.velocity.normalized;

        /** Which direction the character is currently facing */
        private Vector2 _facing = Vector2.right;

        /** Which direction is the character currently facing? */
        public Vector2 Facing => _facing;

        /** The velocity of this character. */
        public Vector2 Velocity => _rigidbody.velocity;

        /** The original assigned gravity scale of the rigidbody */
        private float _originalGravity;

        /** Knockback to be applied on the next physics update */
        private Vector2 _knockback;

        /** Whether this character can currently jump */
        public bool CanJump => _jumpCount < MaxJumps;

        [Tooltip("Gravity scale multiplier of the jump during the upward arc.")]
        [SerializeField] private float upArcGravity;
        [Tooltip("Gravity scale multiplier of the jump during the peak.")]
        [SerializeField] private float jumpPeakGravity;
        [Tooltip("Duration of the gravity scale at the peak.")]
        [SerializeField] private float jumpPeakDuration;
        [Tooltip("Gravity scale multiplier of the jump during the downward arc.")]
        [SerializeField] private float downArcGravity;

        [Tooltip("Time it takes for the player to reach their max speed when they start moving.")]
        [SerializeField] private float accelerationTime = 0.05f;
        [Tooltip("Time it takes for the player to reach zero speed when they stop moving.")]
        [SerializeField] private float decelerationTime = 0.05f;

        private IEnumerator _jumpFunction;
        private IEnumerator _velocityDamp;

        /** Can this character move? */
        public bool CanMove { get; set; } = true;

        /** Reference to this character's animator*/
        public Animator Animator => _anim;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
            _audio = GetComponent<AudioSource>();
        }

        protected virtual void Start()
        {

            _audio.loop = true;

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

            // Move using velocity based on the cached movement rates
            Vector2 targetVelocity = Vector2.zero;

            if (CanMove)
            {

                float movementRate = MovementSpeed * Time.fixedDeltaTime * 10.0f;
                switch (MovementMode)
                {
                    case ECharacterMovementMode.Walking:
                        Vector2 velocity = _rigidbody.velocity;
                        targetVelocity = new Vector2(_horizontalMovementRate * movementRate, velocity.y);
                        /*float acceleration = 0.0f;
                        if (_rigidbody.velocity.magnitude < targetVelocity.magnitude)
                        {
                            acceleration = _horizontalMovementRate * movementRate * Time.fixedDeltaTime / accelerationTime;
                            _rigidbody.velocity = new Vector2(Mathf.Clamp(velocity.x + acceleration, -movementRate, movementRate), velocity.y);
                        }
                        else if(_rigidbody.velocity.magnitude > targetVelocity.magnitude)
                        {
                            acceleration = movementRate * Time.fixedDeltaTime / decelerationTime * Mathf.Sign(_facing.x);
                            float newXVelocity = velocity.x + acceleration;
                            _rigidbody.velocity = new Vector2(Mathf.Sign(newXVelocity) != Mathf.Sign(_facing.x) ? newXVelocity : 0, velocity.y);
                        }*/
                        break;
                    case ECharacterMovementMode.Flying:
                        targetVelocity = new Vector2(_horizontalMovementRate, _verticalMovementRate) * movementRate;
                        break;
                    default:
                        targetVelocity = _rigidbody.velocity;
                        break;
                }

                float dampTime = _rigidbody.velocity.magnitude < targetVelocity.magnitude ? accelerationTime : decelerationTime;

                _rigidbody.velocity = Vector2.SmoothDamp(_rigidbody.velocity, targetVelocity, ref _tempVelocity, dampTime) + _knockback;
            }

            // Play or stop audio based on whether the character is moving in the way their movement mode specifies
            bool moving = (Mathf.Abs(targetVelocity.x) > 0.000000001f && IsGrounded && movementMode == ECharacterMovementMode.Walking)
                || (targetVelocity.magnitude > 0.000000001f && movementMode == ECharacterMovementMode.Flying);
            if(moving && !_audio.isPlaying)
            {
                _audio.Play();
            }
            else if (!moving && _audio.isPlaying)
            {
                _audio.Stop();
            }

            // Set InMotion variable so other scripts can see if the character is moving
            if (targetVelocity.magnitude > 0.000000001f && !_inMotion)
            {
                _inMotion = true;
            }
            else if (targetVelocity.magnitude <= 0.000000001f && _inMotion)
            {
                _inMotion = false;
            }

            //_rigidbody.velocity += _knockback;
            _knockback = Vector2.zero;

            // Update the direction the character is facing if it has changed
            Vector2 dir = targetVelocity.normalized;
            if (dir.x != 0.0f)
            {
                bool isRight = dir.x > 0.0f;
                _facing = isRight ? Vector2.right : Vector2.left;
                _sprite.flipX = flipSprite ? isRight : !isRight;
            }

            UpdateAnimation(dir);
        }

        protected virtual void UpdateAnimation(Vector2 direction)
        {
            
        }

        private IEnumerator VelocityDamp(Vector2 targetVelocity, float dampTime)
        {
            while(targetVelocity != _rigidbody.velocity)
            {
                Vector2 currentVelocity = _rigidbody.velocity;
                _rigidbody.velocity = IsWalking ?
                    new Vector2(Mathf.SmoothDamp(currentVelocity.x, targetVelocity.x, ref _tempXVelocity, dampTime), currentVelocity.y) :
                    Vector2.SmoothDamp(currentVelocity, targetVelocity, ref _tempVelocity, dampTime);
                yield return new WaitForFixedUpdate();
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
         * Zeroes out the movement for this character.
         */
        public void ClearMovement()
        {
            _horizontalMovementRate = 0.0f;
            _verticalMovementRate = 0.0f;
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

        public void ApplyKnockback(Vector2 force)
        {
            _knockback += force;
        }

        /**
         * Makes the character jump if the character is grounded or has available multi-jumps.
         * This will only work for characters with a Walking movement mode.
         */
        public virtual void Jump()
        {
            // If the player has jumps available and is in a grounded movement mode, jump
            if (CanJump)
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0); // Clear any existing vertical velocity
                _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse); // Add the impulse
                _jumpCount++; // Update the jump count
                _jumpForce *= JumpForceFalloff; // Apply the force falloff
                if(_jumpFunction != null) StopCoroutine(_jumpFunction); // Stop jump coroutine of any previous jumps
                _jumpFunction = JumpPhysics();
                StartCoroutine(_jumpFunction); // Start a new jump coroutine
            }
        }

        /**
         * Handles any changes to jump physics during the jump
         */
        private IEnumerator JumpPhysics()
        {
            // Make sure gravity scale is set to original value in case it had been changed by a different jump
            SetGravity(_originalGravity * upArcGravity);

            // The period of the time before the top of the arc (the top of the arc is represented by the moment y velocity = 0)
            while (_rigidbody.velocity.y > 0)
            {
                yield return null;
            }

            // Change the gravity scale at the peak of the jump for the specified number of seconds
            AtJumpPeak();
            SetGravity(_originalGravity * jumpPeakGravity);
            yield return new WaitForSeconds(jumpPeakDuration);

            // Change the gravity scale on the downaward arc of the jump
            SetGravity(_originalGravity * downArcGravity);

            while(_rigidbody.velocity.y < 0)
            {
                yield return null;
            }

            // Reset gravity scale to original value
            SetGravity(_originalGravity);
        }

        protected virtual void AtJumpPeak()
        {

        }
        
        private void SetGravity(float newGravity)
        {
            _rigidbody.gravityScale = newGravity;
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
        protected virtual void CheckGrounded()
        {
            
            // Note: Character can only be grounded if in walking mode
            _isGrounded = Physics2D.BoxCast(RootPosition, new Vector2(_extents.x * 2, 0.1f),
                0, Vector2.down, 0.07f, 
                Physics2D.AllLayers & ~LayerMask.GetMask("Player", "Ignore Raycast")) && IsWalking;

        }
    }
}
