using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace BuildABot
{

    /**
     * The pathing mode used by an AI agent.
     */
    public enum EPathingMode
    {
        /** The enemy will patrol between waypoints. */
        Patrolling,
        /** The enemy will seek a specific topic. */
        Seeking
    }

    /**
     * The AI controller used to drive enemy behaviors.
     */
    [RequireComponent(typeof(Seeker), typeof(Rigidbody2D), typeof(EnemyMovement))]
    public class EnemyController : MonoBehaviour
    {

        [Header("Enemy Behavior Mode")]
        [SerializeField] private EPathingMode enemyMode = EPathingMode.Patrolling;

        [Header("Patrolling Information")]

        [Tooltip("List of waypoints for patrolling character behavior")]
        [SerializeField] private List<Waypoint> patrolPoints;

        [Tooltip("The tolerance value used to determine how close the enemy must get to a patrol point.")]
        [SerializeField] private float nextPatrolPointDistance = 1f;

        /** The current patrol point target index. */
        private int _currentPatrolPoint;

        /** The field of view component used for vision based detection. */
        private FieldOfView _fov;

        [Header("Seeking Information")]

        [Tooltip("Enable or Disable seeking for this enemy type")]
        [SerializeField] private bool canSeek;

        [Tooltip("The target for the enemy to seek")]
        [SerializeField] private Transform target;


        [Tooltip("How close an enemy needs to be to a waypoint to move on to the next one")]
        [SerializeField] private float nextWaypointDistance = 3f;

        /** The path that the enemy will be following. */
        private Path _path;
        /** Index of the enemy's current waypoint along the path. */
        private int _currentWaypoint;
        /** Boolean value for whether or not the enemy has reached the end of its path. */
        private bool _reachedEndOfPath;

        [Tooltip("Interval in seconds to update enemy pathing")]
        [SerializeField] private float pathUpdateInterval = 0.5f;

        /** The movement controller used by this enemy. */
        private EnemyMovement _enemyMovement;
        /** The Seeker component that we are using */
        private Seeker _seeker;
        /** The enemy's rigidbody */
        private Rigidbody2D _rigidbody;

        /** The IEnumerator used by the UpdatePath coroutine. */
        private IEnumerator _updatePathCoroutine;

        /** Reference to the enemy's current mode */
        public EPathingMode EnemyMode => enemyMode;

        /** Reference to the enemy's seeking ability */
        public bool CanSeek => canSeek;

        void Start()
        {
            //Get component values
            _seeker = GetComponent<Seeker>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _fov = GetComponent<FieldOfView>();
            _enemyMovement = GetComponent<EnemyMovement>();

            _fov.StartLooking();
        }

        void OnPathComplete(Path p)
        {
            if (!p.error)
            {
                _path = p;
                _currentWaypoint = 0;
            }
        }

        void UpdatePath()
        {
            if (_seeker.IsDone())
                _seeker.StartPath(_rigidbody.position, target.position, OnPathComplete);
        }

        void Update()
        {
            switch (enemyMode)
            {
                case EPathingMode.Patrolling:
                    PatrollingStep();
                    break;
                case EPathingMode.Seeking:
                    SeekingStep();
                    break;
            }

            //Update player direction
            //Values here might need to change depending on which way our sprites face by default
            if (_enemyMovement.MovementDirection.x > 0.01f)
            {
                _fov.flipx = false;
            }
            else if (_enemyMovement.MovementDirection.x < -0.01f)
            {
                _fov.flipx = true;
            }

        }

        void PatrollingStep()
        {

            //Check to see if there's a target in our field of view
            if (canSeek && _fov.visibleTargets.Count > 0)
            {
                AddTarget(_fov.visibleTargets[0]);
                return;
            }

            //Exit Conditions
            if (patrolPoints.Count <= 0)
                return;

            //Loop if necessary
            if (_currentPatrolPoint >= patrolPoints.Count)
                _currentPatrolPoint = 0;

            //Move to next waypoint
            _enemyMovement.MoveToPosition(patrolPoints[_currentPatrolPoint].position);

            //Check to see if we've reached the point where we can move to the next waypoint
            float distance = Vector2.Distance(_rigidbody.position, patrolPoints[_currentPatrolPoint].position);
            if (distance < nextPatrolPointDistance)
            {
                _currentPatrolPoint++;
            }
        }

        void SeekingStep()
        {
            //Exit conditions
            if (_path == null)
                return;

            _reachedEndOfPath = _currentWaypoint >= _path.vectorPath.Count;
            if (_reachedEndOfPath) return;

            //Move to next waypoint
            _enemyMovement.MoveToPosition(_path.vectorPath[_currentWaypoint]);

            //Check to see if we've reached the point where we can move to the next waypoint
            float distance = Vector2.Distance(_rigidbody.position, _path.vectorPath[_currentWaypoint]);
            if (distance < nextWaypointDistance)
            {
                _currentWaypoint++;
            }
        }

        public void AddTarget(Transform newTarget)
        {
            //Dumb enemy - chase the first thing it sees
            target = newTarget;
            _fov.StopLooking();
            enemyMode = EPathingMode.Seeking;

            if (_updatePathCoroutine != null) StopCoroutine(_updatePathCoroutine);

            //Start pathing
            _updatePathCoroutine = Utility.RepeatFunction(this, UpdatePath, pathUpdateInterval);

            //Update animator
            _enemyMovement.Animator.SetInteger("EnemyState", 1);
        }
    }
}

