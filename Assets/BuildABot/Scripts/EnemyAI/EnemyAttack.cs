using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class EnemyAttack : MonoBehaviour
    {

        [Header("Attack Information")]
        [Tooltip("Reference to the Attack that this enemy should use")]
        [SerializeField] private AttackData attack;

        [Tooltip("Reference to the GameObject containing graphics for visualizing the attack")]
        [SerializeField] private GameObject attackGraphics;

        /** Reference to the parent's CombatController component */
        private CombatController _combatController;

        /** Reference to the parent's EnemyController component */
        private EnemyController _enemyController;

        /** Reference to this object's generic Collider2D component */
        private Collider2D _collider;

        /** Reference to the parent's EnemyMovement component */
        private EnemyMovement _enemyMovement;

        /** Internal flag to know if an attack is currently running */
        private bool _isAttacking;

        /** Audio source for playing sound effects */
        private AudioSource _audioSource;

        void Awake()
        {
            //Initialize fields
            _combatController = GetComponentInParent<CombatController>();
            _collider = GetComponent<Collider2D>();
            _enemyMovement = GetComponentInParent<EnemyMovement>();
            _enemyController = GetComponentInParent<EnemyController>();
            _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            //Flip offset on melee box
            if (attack is MeleeAttackData melee)
            {
                BoxCollider2D box = _collider as BoxCollider2D;
                if (_enemyMovement.Facing.x <= -0.2f)
                {
                    box.offset = new Vector2(-0.5f, 0);
                    attackGraphics.transform.localPosition = new Vector3(-0.5f, 0, 0);
                }
                else if (_enemyMovement.Facing.x >= 0.2f)
                {
                    box.offset = new Vector2(0.5f, 0);
                    attackGraphics.transform.localPosition = new Vector3(0.5f, 0, 0);
                }

            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isAttacking && other.gameObject.TryGetComponent<Player>(out _))
            {
                _isAttacking = true;
                _combatController.AttackDirection = ((Vector2)(other.transform.position - transform.position)).normalized;
                _combatController.TryPerformAttack(attack, HandleAttackProgress, HandleAttackFinish);

                if (attack is MeleeAttackData melee)
                {
                    attackGraphics.SetActive(true);
                }

                //Target enemy if in range
                if (_enemyController.CanSeek && _enemyController.EnemyMode == EPathingMode.Patrolling)
                {
                    _enemyController.AddTarget(other.transform);
                }
            }
            if (_enemyMovement.MovementMode == ECharacterMovementMode.Walking && other.gameObject.CompareTag("Player"))
            {
                //Turn around
                _enemyController.CurrentPatrolPoint++;
            }
            
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!_isAttacking && other.gameObject.TryGetComponent<Player>(out _) && !(attack is MeleeAttackData))
            {
                _isAttacking = true;
                _combatController.AttackDirection = ((Vector2)(other.transform.position - transform.position)).normalized;
                _combatController.TryPerformAttack(attack, HandleAttackProgress, HandleAttackFinish);
            }
        }

        void HandleAttackProgress(float progress)
        {
            if (attack is AoeAttackData aoe)
            {
                float radius = aoe.AreaOverTime.Evaluate(progress) * aoe.Radius;
                attackGraphics.transform.localScale = new Vector3(radius, radius, 1);
            }
        }

        void HandleAttackFinish(List<Character> characters)
        {
            Debug.Log("Finishing Attack");
            _isAttacking = false;
            if (attack is MeleeAttackData melee)
            {
                attackGraphics.SetActive(false);
            }
        }
    }
}