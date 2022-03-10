using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public class Waypoint : MonoBehaviour
    {
        /** Location of a waypoint in worldspace */
        public Vector3 position => transform.position;


        /* Draw a gizmo so that waypoints can be seen in the scene editor */
        private void OnDrawGizmos ()
        {
            Gizmos.DrawSphere(position, .5f);
        }
    }
}

