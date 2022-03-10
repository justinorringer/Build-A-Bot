using UnityEngine;

namespace BuildABot
{
    
    /**
     * The core data and logic associated with an enemy character.
     */
    [RequireComponent(typeof(EnemyMovement), typeof(EnemyController))]
    public class Enemy : MonoBehaviour
    {

        [Tooltip("The attributes available to this enemy.")]
        [SerializeField] private CharacterAttributeSet attributes;

        /** The attribute set used by this character. */
        public CharacterAttributeSet Attributes => attributes;

        /** The enemy movement component used by this player. */
        private EnemyMovement _enemyMovement;
        /** The enemy controller used by this enemy. */
        private EnemyController _enemyController;
        
        
        protected void Awake()
        {
            attributes.Initialize();
        }
        
        
        protected void Start()
        {
            _enemyMovement = GetComponent<EnemyMovement>();
            _enemyController = GetComponent<EnemyController>();
        }
    }
}