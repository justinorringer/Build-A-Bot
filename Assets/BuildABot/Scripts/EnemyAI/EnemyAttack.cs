using System;
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

        /** Sprite Renderer of the associated enemy */
        private SpriteRenderer _enemySprite;

        /** The initial flipX value for this enemy's sprite */
        private bool _flipXInitial;

        /** The field of view component for the associated enemy */
        private FieldOfView _fov;

        void Awake()
        {
            //Initialize fields
            _combatController = GetComponentInParent<CombatController>();
            _collider = GetComponent<Collider2D>();
            _enemyMovement = GetComponentInParent<EnemyMovement>();
            _enemyController = GetComponentInParent<EnemyController>();
            _enemySprite = GetComponentInParent<SpriteRenderer>();
            _flipXInitial = _enemySprite.flipX;
            _fov = GetComponentInParent<FieldOfView>();
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
            if (other.gameObject.TryGetComponent<Player>(out _))
            {
                if (!_isAttacking)
                {
                    if (attack is MeleeAttackData melee)
                    {
                        AttackCharacter(other);
                    }

                    if (attack is AoeAttackData)
                    {
                        //Target enemy if in range
                        if (_enemyController.CanSeek && (_enemyController.EnemyMode == EPathingMode.Patrolling || _enemyController.EnemyMode == EPathingMode.Returning))
                        {
                            _enemyController.AddTarget(other.transform);
                        }
                        AttackCharacter(other);
                    }

                    if (attack is ProjectileAttackData && _fov.visibleTargets.Count > 0)
                    {
                        AttackCharacter(other);
                    }
                    
                }
                
                if (_enemyMovement.MovementMode == ECharacterMovementMode.Walking)
                {
                    //Turn around
                    _enemyController.CurrentPatrolPoint++;
                }
            }
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<Player>(out _))
            {
                if (!_isAttacking && attack is AoeAttackData)
                {
                    AttackCharacter(other);
                }

                if (attack is ProjectileAttackData)
                {
                    _enemySprite.flipX = other.transform.position.x > transform.position.x;
                    if (!_isAttacking && _fov.visibleTargets.Count > 0)
                    {
                        _isAttacking = true;
                        _combatController.AttackDirection =
                            ((Vector2)(other.transform.position - transform.position)).normalized;
                        _combatController.TryPerformAttack(attack, HandleAttackProgress, HandleAttackFinish);
                        GetComponentInParent<Animator>().SetBool(attack.AnimationTriggerName, true);
                    }
                }
            }
        }

        void AttackCharacter(Collider2D other)
        {
            _isAttacking = true;
            _combatController.AttackDirection =
            ((Vector2)(other.transform.position - transform.position)).normalized;
            _combatController.TryPerformAttack(attack, HandleAttackProgress, HandleAttackFinish);
            GetComponentInParent<Animator>().SetBool(attack.AnimationTriggerName, true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<Player>(out _) && (attack is ProjectileAttackData))
            {
                _enemySprite.flipX = _flipXInitial;
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
            _isAttacking = false;
            if (attack is MeleeAttackData melee)
            {
                attackGraphics.SetActive(false);
            }
            GetComponentInParent<Animator>().SetBool(attack.AnimationTriggerName, false);
        }
    }
}