using UnityEngine;

namespace BuildABot
{
    
    /**
     * the core data and logic associated with the Player character.
     */
    [RequireComponent(typeof(PlayerMovement))]
    public class Player : MonoBehaviour
    {
        
        // TODO: Create a character base class? Coordinate with Enemy and NPC development

        [Tooltip("The attributes available to this player.")]
        [SerializeField] private CharacterAttributeSet attributes;

        /** The attribute set used by this character. */
        public CharacterAttributeSet Attributes => attributes;

        /** The player movement component used by this player. */
        private PlayerMovement _playerMovement;
        /** The player input component used by this player. */
        private PlayerInput _playerInput;

        protected void Awake()
        {
            attributes.Initialize();
            Debug.Log(attributes);
            _playerMovement = GetComponent<PlayerMovement>();
            _playerInput = GetComponent<PlayerInput>();
        }

    }
}