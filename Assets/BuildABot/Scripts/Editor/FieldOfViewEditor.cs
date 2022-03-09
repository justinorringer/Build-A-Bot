using UnityEngine;
using System.Collections;
using UnityEditor;

namespace BuildABot{
    //Source: https://github.com/SebLague/Field-of-View/blob/master/Episode%2001/Editor/FieldOfViewEditor.cs
    [CustomEditor (typeof (FieldOfView))]
    public class FieldOfViewEditor : Editor {

        void OnSceneGUI() {
            FieldOfView fow = (FieldOfView)target;
            Handles.color = Color.white;
            Handles.DrawWireArc (fow.transform.position, Vector3.forward, Vector3.right, 360, fow.viewRadius);
            Vector2 viewAngleA = fow.DirFromAngle (-fow.viewAngle / 2, false);
            Vector2 viewAngleB = fow.DirFromAngle (fow.viewAngle / 2, false);

            if (fow.flipx)
            {
                viewAngleA *= -1;
                viewAngleB *= -1;
            }

            Handles.DrawLine (fow.transform.position, (Vector2) fow.transform.position + viewAngleA * fow.viewRadius);
            Handles.DrawLine (fow.transform.position, (Vector2) fow.transform.position + viewAngleB * fow.viewRadius);

            Handles.color = Color.red;
            foreach (Transform visibleTarget in fow.visibleTargets) {
                Handles.DrawLine (fow.transform.position, visibleTarget.position);
            }
        }

    }
}
