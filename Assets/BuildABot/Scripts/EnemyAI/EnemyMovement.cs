using UnityEngine;

namespace BuildABot
{
    /**
     * The movement controller used by enemies.
     */
    [RequireComponent(typeof(EnemyController), typeof(Enemy))]
    public class EnemyMovement : CharacterMovement
    {

        /** The enemy using this movement component. */
        private Enemy _enemy;

        protected override CharacterAttributeSet SourceAttributes => _enemy.Attributes;
        
        protected override void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }
    }
}