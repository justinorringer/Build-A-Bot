using UnityEngine;

namespace BuildABot
{
    /**
     * The movement controller used by players.
     */
    [RequireComponent(typeof(PlayerController), typeof(Player))]
    public class PlayerMovement : CharacterMovement
    {

        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip jumpSound;

        /** The player using this movement component. */
        private Player _player;

        protected override CharacterAttributeSet SourceAttributes => _player.Attributes;

        /** Hash of "running" parameter in the animator, stored for optimization */
        private int _runningBoolHash;
        /** Hash of "idle" parameter in the animator, stored for optimization */
        private int _idleBoolHash;
        /** Hash of "grounded" parameter in the animator, stored for optimization */
        private int _groundedBoolHash;
        /** Hash of "jump" parameter in the animator, stored for optimization */
        private int _jumpTriggerHash;
        /** Hash of "jump peaked" parameter in the animator, stored for optimization */
        private int _jumpPeakedTriggerHash;

        [Tooltip("Particle system to be played when this character lands")]
        [SerializeField] private ParticleSystem landParticles;

        protected override void Awake()
        {
            base.Awake();
            _player = GetComponent<Player>();

            _runningBoolHash = Animator.StringToHash("Running");
            _idleBoolHash = Animator.StringToHash("Idle");
            _groundedBoolHash = Animator.StringToHash("Grounded");
            _jumpTriggerHash = Animator.StringToHash("Jump");
            _jumpPeakedTriggerHash = Animator.StringToHash("JumpPeaked");
        }

        protected override void UpdateAnimation(Vector2 direction)
        {
            base.UpdateAnimation(direction);
            if (Animator != null && Animator.runtimeAnimatorController != null)
            {
                // Tell animator if player is running
                Animator.SetBool(_runningBoolHash, direction.x != 0.0f && IsGrounded);
                // Tell animator if player is idle
                Animator.SetBool(_idleBoolHash, Velocity == Vector2.zero);
            }
        }

        public override void Jump()
        {
            if (CanJump)
            {
                Animator.SetTrigger(_jumpTriggerHash);
                source.PlayOneShot(jumpSound);
                GameManager.GameState.TotalJumps++;
            }

            base.Jump();
        }

        protected override void AtJumpPeak()
        {
            base.AtJumpPeak();
            Animator.SetTrigger(_jumpPeakedTriggerHash);
        }

        protected override void CheckGrounded()
        {
            bool wasGrounded = IsGrounded;

            base.CheckGrounded();

            if (Animator != null && Animator.runtimeAnimatorController != null)
                Animator.SetBool(_groundedBoolHash, IsGrounded);

            if (!wasGrounded && IsGrounded)
            {
                landParticles?.Play();
            }
        }
    }
}
