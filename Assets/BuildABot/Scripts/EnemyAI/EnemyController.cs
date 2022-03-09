using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace BuildABot 
{
    [RequireComponent(typeof(Seeker), typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyMode{Patrolling, Seeking}

        [HeaderAttribute("Enemy Behavior Mode")]
        public EnemyMode enemyMode = EnemyMode.Patrolling;

        [HeaderAttribute("Patrolling Information")]

        [Tooltip("List of waypoints for patrolling character behavior")]
        [SerializeField] List<Waypoint> patrolPoints;

        [Tooltip("Speed at which enemy will patrol")]
        [SerializeField] float patrolSpeed = 200f;

        [SerializeField] float nextPatrolPointDistance = 1f;

        int currentPatrolPoint = 0;

        [SerializeField] FieldOfView fov;

        [HeaderAttribute("Seeking Information")]

        [Tooltip("The target for the enemy to seek")]
        [SerializeField] Transform target;

        [Tooltip("Speed at which the enemy moves")]
        [SerializeField] float speed = 200f;

        [Tooltip("How close an enemy needs to be to a waypoint to move on to the next one")]
        [SerializeField] float nextWaypointDistance = 3f;

        //The path that the enemy will be following
        Path path;
        //Index of the enemy's current waypoint along the path
        int currentWaypoint = 0;
        //Boolean value for whether or not the enemy has reached the end of its path
        bool reachedEndOfPath = false;

        [Tooltip("Interval in seconds to update enemy pathing")]
        [SerializeField] float pathUpdateInterval = 0.5f;
        
        //The Seeker component that we are using
        Seeker seeker;
        //The enemy's rigidbody
        Rigidbody2D rb;

        [Tooltip("Reference to the transform with this enemy's graphic elements")]
        [SerializeField] Transform enemyGFX;


        //Private internal variables used for calculation
        Vector2 direction;
        Vector2 force;
        

        void Start()
        {
            //Get component values
            seeker = GetComponent<Seeker>();
            rb = GetComponent<Rigidbody2D>();
            fov = GetComponent<FieldOfView>();


            fov.StartLooking();
        }

        void OnPathComplete(Path p)
        {
            if (!p.error)
            {
                path = p;
                currentWaypoint = 0;
            }
        }

        void UpdatePath()
        {
            if (seeker.IsDone())
                seeker.StartPath(rb.position, target.position, OnPathComplete);
        }

        void FixedUpdate()
        {
            switch(enemyMode)
            {
                case EnemyMode.Patrolling:
                    PatrollingStep();
                    break;
                case EnemyMode.Seeking:
                    SeekingStep();
                    break;
                default:
                    break;
            }

            //Update player direction
            //Values here might need to change depending on which way our sprites face by default
            if (force.x > 0.01f)
            {
                fov.flipx = false;
                enemyGFX.localScale = new Vector3(1f, 1f, 1f);
            } else if (force.x < -0.01f)
            {
                fov.flipx = true;
                enemyGFX.localScale = new Vector3(-1f, 1f, 1f);
            }
            
        }

        void PatrollingStep()
        {
            //Exit Conditions
            if (patrolPoints.Count <= 0)
                return;
            
            //Loop if necessary
            if (currentPatrolPoint >= patrolPoints.Count)
                currentPatrolPoint = 0;

            //Move to next waypoint
            direction = ((Vector2) patrolPoints[currentPatrolPoint].position - rb.position).normalized;
            force = direction * patrolSpeed * Time.deltaTime;
            rb.AddForce(force);

            //Check to see if we've reached the point where we can move to the next waypoint
            float distance = Vector2.Distance(rb.position, patrolPoints[currentPatrolPoint].position);
            if (distance < nextPatrolPointDistance)
            {
                currentPatrolPoint++;
            }
            
            //Check to see if there's a target in our field of view
            if (fov.visibleTargets.Count > 0)
            {
                //Dumb enemy - chase the first thing it sees
                target = fov.visibleTargets[0];
                fov.StopLooking();
                enemyMode = EnemyMode.Seeking;

                //Start pathing
                InvokeRepeating("UpdatePath", 0f, 0.5f);
            }

        }

        void SeekingStep()
        {
            //Exit conditions
            if (path == null)
                return;
            
            if (currentWaypoint >= path.vectorPath.Count)
            {
                reachedEndOfPath = true;
                return;
            } else 
            {
                reachedEndOfPath = false;
            }

            //Move to next waypoint
            direction = ((Vector2) path.vectorPath[currentWaypoint] - rb.position).normalized;
            force = direction * speed * Time.deltaTime;
            rb.AddForce(force);

            //Check to see if we've reached the point where we can move to the next waypoint
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }
        }
    }
}

