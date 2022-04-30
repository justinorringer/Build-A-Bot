using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot{

    //Source: https://github.com/SebLague/Field-of-View/blob/master/Episode%2001/Scripts/FieldOfView.cs
    public class FieldOfView : MonoBehaviour
    {
        [Tooltip("Metrics to control a character's field of view")]
        public float viewRadius = 2f;
        [Range(0, 365)]
        public float viewAngle = 45;


        [Tooltip("Layer Masks to control how a character sees")]
        [SerializeField] LayerMask targetMask;
        [SerializeField] LayerMask obstacleMask;

        [Tooltip("Delay in updating character vision")]
        [SerializeField] float visionDelay = 0.2f;
        
        [Tooltip("Max number of targets in vision")]
        [SerializeField] private int maxTargets = 10;

        /** List that holds transforms of all visible targets */
        public List<Transform> visibleTargets = new List<Transform>();

        public bool flipx;

        bool looking = false;

        /**
        * This function starts the coroutine that updates player vision
        * The visibleTargets list will not begin updating without this function being called.
        */
        public void StartLooking()
        {
            looking = true;
            StartCoroutine(nameof(FindTargetsWithDelay), visionDelay);
            //Debug.Log("Started Looking");
        }

        /**
        * this function stops the coroutine that updates player vision
        * The visibleTargets list will be cleared and stop being updated
        */
        public void StopLooking()
        {
            visibleTargets.Clear();
            looking = false;
            StopCoroutine(nameof(FindTargetsWithDelay));
        }

        IEnumerator FindTargetsWithDelay(float delay)
        {
            //Debug.Log("Started Coroutine");
            while(looking)
            {
                yield return new WaitForSeconds(delay);
                FindVisibleTargets();
            }
        }

        void FindVisibleTargets()
        {
            visibleTargets.Clear();
            
            //Using NonAlloc method to avoid memory leak issue
            Collider2D[] targetsInViewRadius = new Collider2D[maxTargets];
            int size = Physics2D.OverlapCircleNonAlloc(transform.position, viewRadius, targetsInViewRadius, targetMask);
            
            for (int i = 0; i < size; i++) {
                Transform target = targetsInViewRadius[i].transform;
                Vector2 dirToTarget = (target.position - transform.position).normalized;
                Vector3 angleToUse = flipx ? transform.right * -1 : transform.right;
                if (Vector2.Angle (angleToUse, dirToTarget) < viewAngle / 2) {
                    float dstToTarget = Vector2.Distance (transform.position, target.position);
                    if (!Physics2D.Raycast (transform.position, dirToTarget, dstToTarget, obstacleMask)) {
                        visibleTargets.Add (target);
                    }
                }
            }
        }

        /** Utility function to return a direction for a given angle */
        public Vector2 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
            if (!angleIsGlobal) {
                angleInDegrees += transform.eulerAngles.z;
            }
            return new Vector2(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad),Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
        }
    }
}