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

        [Tooltip("The amount of currency that this enemy should drop upon death.")]
        [Min(0)]
        [SerializeField] private int currencyToDrop;

        public override CharacterMovement CharacterMovement => _enemyMovement;

        /** The enemy controller used by this enemy. */
        public EnemyController EnemyController => _enemyController;

        /** The amount of currency dropped by this character. */
        public int DroppedCurrency => currencyToDrop;

        protected override void Awake()
        {
            base.Awake();
            Attributes.Initialize();
        }


        protected override void Start()
        {
            base.Start();
            _enemyMovement = GetComponent<EnemyMovement>();
            _enemyController = GetComponent<EnemyController>();
        }

    }
}