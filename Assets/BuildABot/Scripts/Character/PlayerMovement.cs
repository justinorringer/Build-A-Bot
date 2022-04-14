using UnityEngine;

namespace BuildABot
{
    /**
     * The movement controller used by players.
     */
    [RequireComponent(typeof(PlayerController), typeof(Player))]
    public class PlayerMovement : CharacterMovement
    {

        /** The player using this movement component. */
        private Player _player;

        protected override CharacterAttributeSet SourceAttributes => _player.Attributes;

        /** Hash of "running" parameter in the animator, stored for optimization */
        private int _runningBoolHash;
        /** Hash of "idle" parameter in the animator, stored for optimization */
        private int _idleBoolHash;
        /** Hash of "grounded" parameter in the animator, stored for optimization */
        private int _groundedBoolHash;
        
        protected override void Awake()
        {
            base.Awake();
            _player = GetComponent<Player>();

            _runningBoolHash = Animator.StringToHash("Running");
            _idleBoolHash = Animator.StringToHash("Idle");
            _groundedBoolHash = Animator.StringToHash("Grounded");
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

        protected override void CheckGrounded()
        {
            base.CheckGrounded();

            if (Animator != null && Animator.runtimeAnimatorController != null)
                Animator.SetBool(_groundedBoolHash, IsGrounded);
        }
    }
}
