using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace BuildABot
{
    public class EnemySpawner : MonoBehaviour
    {
        private enum EnemyType{Flying, Turret, Walking}
    
        [Header("General Enemy Information")]
        [Tooltip("All types of flying enemies that can spawn")]
        [SerializeField] private List<GameObject> flyingEnemies;
    
        [Tooltip("All types of turret enemies that can spawn")]
        [SerializeField] private List<GameObject> turretEnemies;
    
        [Tooltip("All types of walking enemies that can spawn")]
        [SerializeField] private List<GameObject> walkingEnemies;

        [Header("Details for Spawned Enemy")]
        [Tooltip("The type of enemy that should be spawned")]
        [SerializeField] private EnemyType enemyToSpawn;
    
        [Tooltip("The patrol points to be used by a walking or flying enemy")]
        [SerializeField] private List<Waypoint> patrolPoints;

        private void Start()
        {
            //Logic will be - spawn enemy of game stage before level 4, afterwards do random
            int stage = GameManager.GameState.GameStage;
            int variant;
            if (stage >= 3) //4 is post-game
            {
                 variant = new Random().Next(0, stage);
            }
            else
            {
                variant = stage;
            }

            SpawnEnemy(variant);
        }

        //0 for normal, 1 for frozen, 2 for advanced
        private void SpawnEnemy(int variant)
        {
            Debug.Log("Spawning Enemy");
            GameObject g;
            switch (enemyToSpawn)
            {
                case EnemyType.Flying:
                    g = Instantiate(flyingEnemies[variant], transform.position, transform.rotation);
                    g.GetComponent<EnemyController>().setPatrolPoints(patrolPoints);
                    break;
                case EnemyType.Turret:
                    Instantiate(turretEnemies[variant], transform.position, transform.rotation);
                    break;
                case EnemyType.Walking:
                    g = Instantiate(walkingEnemies[variant], transform.position, transform.rotation);
                    g.GetComponent<EnemyController>().setPatrolPoints(patrolPoints);
                    break;
            }
        }
    }
}
