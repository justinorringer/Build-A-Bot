using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class EnemyAttack : MonoBehaviour
    {

        private CombatController _combatController;

        [SerializeField] private AttackData attack;

        void Awake()
        {
            _combatController = GetComponentInParent<CombatController>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<Player>(out _))
            {
                if (_combatController.TryPerformAttack(attack, HandleAttackProgress))
                {

                }
            }
        }

        void HandleAttackProgress(float progress)
        {
            AoeAttackData aoe = attack as AoeAttackData;
            if (aoe == null) return;
        }
    }
}