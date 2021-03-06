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

        [Tooltip("The particle system prefab used when the enemy dies.")]
        [SerializeField] private ParticleSystem deathParticle;

        [Tooltip("The amount of currency that this enemy should drop upon death.")]
        [Min(0)]
        [SerializeField] private int currencyToDrop;

        [Tooltip("The sound clip played when the enemy dies.")]
        [SerializeField] private AudioClip deathClip;

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

        protected override void Kill()
        {
            PlayDeathParticle();
            if (deathClip != null)
                AudioSource.PlayClipAtPoint(deathClip, transform.position);
            base.Kill();
        }

        public void PlayDeathParticle()
        {
            Vector3 pos = transform.position;
            pos.z = deathParticle.transform.position.z;
            ParticleSystem ps = Instantiate(deathParticle, pos, Quaternion.identity);
        }

    }
}