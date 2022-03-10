using UnityEngine;

namespace BuildABot
{
    /**
     * The movement controller used by players.
     */
    [RequireComponent(typeof(PlayerInput), typeof(Player))]
    public class PlayerMovement : CharacterMovement
    {

        /** The player using this movement component. */
        private Player _player;

        protected override CharacterAttributeSet SourceAttributes => _player.Attributes;
        
        protected override void Awake()
        {
            _player = GetComponent<Player>();
        }
    }
}
