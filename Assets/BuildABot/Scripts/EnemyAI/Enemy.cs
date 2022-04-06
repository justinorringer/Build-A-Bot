using UnityEngine;

namespace BuildABot
{

    /**
     * The core data and logic associated with an enemy character.
     */
    [RequireComponent(typeof(EnemyMovement), typeof(EnemyController))]
    public class Enemy : Character
    {

        /** The enemy movement component used by this player. */
        private EnemyMovement _enemyMovement;
        /** The enemy controller used by this enemy. */
        private EnemyController _enemyController;

        /** The amount of currency that this enemy should drop upon death */
        public int currencyToDrop;

        public override CharacterMovement CharacterMovement => _enemyMovement;

        /** The enemy controller used by this enemy. */
        public EnemyController EnemyController => _enemyController;

        protected override void Awake()
        {
            base.Awake();
            Attributes.Initialize();
        }


        protected void Start()
        {
            _enemyMovement = GetComponent<EnemyMovement>();
            _enemyController = GetComponent<EnemyController>();
        }
    }
}