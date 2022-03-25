using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    public static class DebugUtility
    {
        /**
         * Draws the shape specified by the provided points. Debug only.
         * <param name="points">The points to draw. At least two points are required.</param>
         * <param name="connectEndpoints">Should the shape have the first and last point connected? Defaults to true.</param>
         */
        public static void DrawShape(IList<Vector3> points, bool connectEndpoints = true)
        {
            DrawShape(points, connectEndpoints, Color.white);
        }
        
        /**
         * Draws the shape specified by the provided points. Debug only.
         * <param name="points">The points to draw. At least two points are required.</param>
         * <param name="connectEndpoints">Should the shape have the first and last point connected?</param>
         * <param name="color">The color of the shape to draw.</param>
         * <param name="duration">The duration to draw the shape for. A value of 0 will draw for one frame. Defaults to 0.</param>
         * <param name="depthTest">Should the shape be depth tested? Defaults to true.</param>
         */
        public static void DrawShape(IList<Vector3> points, bool connectEndpoints, Color color, float duration = 0.0f, bool depthTest = true)
        {
            if (null == points || points.Count < 2) return;
            int lastIndex = points.Count - 1;
            for (int i = 0; i < lastIndex; i++)
            {
                Debug.DrawLine(points[i], points[i + 1], color, duration, depthTest);
            }
            if (connectEndpoints) Debug.DrawLine(points[lastIndex], points[0], color, duration, depthTest);
        }

        /**
         * Draws a 2D box in the scene at the specified depth. Debug only.
         * <param name="center">The center of the box to draw.</param>
         * <param name="size">The size of the box to draw.</param>
         * <param name="angle">The angle to draw the box at. Defaults to 0.</param>
         * <param name="depth">The z depth of the box to draw. Defaults to 0.</param>
         */
        public static void DrawBox2D(Vector2 center, Vector2 size, float angle = 0.0f, float depth = 0.0f)
        {
            DrawBox2D(center, size, angle, depth, Color.white);
        }

        /**
         * Draws a 2D box in the scene at the specified depth. Debug only.
         * <param name="center">The center of the box to draw.</param>
         * <param name="size">The size of the box to draw.</param>
         * <param name="angle">The angle to draw the box at.</param>
         * <param name="depth">The z depth of the box to draw.</param>
         * <param name="color">The color of the shape to draw.</param>
         * <param name="duration">The duration to draw the shape for. A value of 0 will draw for one frame. Defaults to 0.</param>
         * <param name="depthTest">Should the shape be depth tested? Defaults to true.</param>
         */
        public static void DrawBox2D(Vector2 center, Vector2 size, float angle, float depth, Color color, float duration = 0.0f,
            bool depthTest = true)
        {
            Vector2 halfSize = size * 0.5f;
            Vector2 toLowerLeft = (-halfSize).Rotate(angle);
            Vector2 toUpperLeft = new Vector2(-halfSize.x, halfSize.y).Rotate(angle);
            Vector2 ll = center + toLowerLeft;
            Vector2 ul = center + toUpperLeft;
            Vector2 ur = center - toLowerLeft;
            Vector2 lr = center - toUpperLeft;
            DrawShape(new [] {
                    new Vector3(ll.x, ll.y, depth),
                    new Vector3(ul.x, ul.y, depth),
                    new Vector3(ur.x, ur.y, depth),
                    new Vector3(lr.x, lr.y, depth)
            }, true, color, duration, depthTest);
        }

        /**
         * Draws a 2D boxcast in the scene. Debug only.
         * <param name="origin">The origin of the boxcast.</param>
         * <param name="size">The size of the box being cast.</param>
         * <param name="angle">The angle of the box being cast.</param>
         * <param name="direction">The direction that the box is cast in.</param>
         * <param name="distance">The distance that the box travels.</param>
         */
        public static void DrawBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance)
        {
            DrawBoxCast2D(origin, size, angle, direction, distance, Color.white);
        }

        /**
         * Draws a 2D boxcast in the scene. Debug only.
         * <param name="origin">The origin of the boxcast.</param>
         * <param name="size">The size of the box being cast.</param>
         * <param name="angle">The angle of the box being cast.</param>
         * <param name="direction">The direction that the box is cast in.</param>
         * <param name="distance">The distance that the box travels.</param>
         * <param name="color">The color of the shape to draw.</param>
         * <param name="duration">The duration to draw the shape for. A value of 0 will draw for one frame. Defaults to 0.</param>
         * <param name="depthTest">Should the shape be depth tested? Defaults to true.</param>
         */
        public static void DrawBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance,
            Color color, float duration = 0.0f, bool depthTest = true)
        {
            // Draw starting box
            Vector2 halfSize = size * 0.5f;
            Vector2 toLowerLeft = (-halfSize).Rotate(angle);
            Vector2 toUpperLeft = new Vector2(-halfSize.x, halfSize.y).Rotate(angle);
            Vector2 ll = origin + toLowerLeft;
            Vector2 ul = origin + toUpperLeft;
            Vector2 ur = origin - toLowerLeft;
            Vector2 lr = origin - toUpperLeft;
            DrawShape(new Vector3[] {
                ll, ul, ur, lr
            }, true, color, duration, depthTest);

            Vector2 delta = direction.normalized * distance;
            
            // Draw finishing box
            Vector2 newLl = ll + delta;
            Vector2 newUl = ul + delta;
            Vector2 newUr = ur + delta;
            Vector2 newLr = lr + delta;
            DrawShape(new Vector3[] {
                newLl,
                newUl,
                newUr,
                newLr
            }, true, color, duration, depthTest);
            
            // Draw connections
            Debug.DrawLine(ll, newLl, color, duration, depthTest);
            Debug.DrawLine(ul, newUl, color, duration, depthTest);
            Debug.DrawLine(ur, newUr, color, duration, depthTest);
            Debug.DrawLine(lr, newLr, color, duration, depthTest);
        }

        /**
         * Draws a 2D circle in the scene. Debug only.
         * <param name="origin">The origin of the circle.</param>
         * <param name="radius">The radius of the circle.</param>
         * <param name="depth">The z depth to draw the circle at. Defaults to 0.</param>
         * <param name="segments">The number of line segments to build the circle from. Defaults to 16.</param>
         */
        public static void DrawCircle2D(Vector2 origin, float radius, float depth = 0.0f, int segments = 16)
        {
            DrawCircle2D(origin, radius, depth, segments, Color.white);
        }

        /**
         * Draws a 2D circle in the scene. Debug only.
         * <param name="origin">The origin of the circle.</param>
         * <param name="radius">The radius of the circle.</param>
         * <param name="depth">The z depth to draw the circle at.</param>
         * <param name="segments">The number of line segments to build the circle from.</param>
         * <param name="color">The color of the shape to draw.</param>
         * <param name="duration">The duration to draw the shape for. A value of 0 will draw for one frame. Defaults to 0.</param>
         * <param name="depthTest">Should the shape be depth tested? Defaults to true.</param>
         */
        public static void DrawCircle2D(Vector2 origin, float radius, float depth, int segments, Color color, float duration = 0.0f,
            bool depthTest = true)
        {
            if (segments < 3) return;
            Vector3[] points = new Vector3[segments];
            float arc = 360.0f / segments;
            for (int i = 0; i < segments; i++)
            {
                points[i] = Vector2.up.Rotate(arc * i) * radius + origin;
                points[i].z = depth;
            }
            DrawShape(points, true, color, duration, depthTest);
        }

        /**
         * Draws a 2D circle cast in the scene. Debug only.
         * <param name="origin">The origin of the circle cast.</param>
         * <param name="radius">The radius of the circle being cast.</param>
         * <param name="direction">The direction that the box is cast in.</param>
         * <param name="distance">The distance that the box travels.</param>
         * <param name="segments">The number of line segments to build the circle from.</param>
         */
        public static void DrawCircleCast2D(Vector2 origin, float radius, Vector2 direction, float distance, int segments = 16)
        {
            DrawCircleCast2D(origin, radius, direction, distance, segments, Color.white);
        }

        /**
         * Draws a 2D circle cast in the scene. Debug only.
         * <param name="origin">The origin of the circle cast.</param>
         * <param name="radius">The radius of the circle being cast.</param>
         * <param name="direction">The direction that the box is cast in.</param>
         * <param name="distance">The distance that the box travels.</param>
         * <param name="segments">The number of line segments to build the circle from.</param>
         * <param name="color">The color of the shape to draw.</param>
         * <param name="duration">The duration to draw the shape for. A value of 0 will draw for one frame. Defaults to 0.</param>
         * <param name="depthTest">Should the shape be depth tested? Defaults to true.</param>
         */
        public static void DrawCircleCast2D(Vector2 origin, float radius, Vector2 direction, float distance,
            int segments, Color color, float duration = 0.0f, bool depthTest = true)
        {
            if (segments < 3) return;
            float arc = 360.0f / segments;
            
            // Draw starting circle
            Vector3[] startingPoints = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                startingPoints[i] = Vector2.up.Rotate(arc * i) * radius + origin;
            }
            DrawShape(startingPoints, true, color, duration, depthTest);
            // Draw final circle
            Vector3 delta = direction * distance;
            Vector3[] finalPoints = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                finalPoints[i] = startingPoints[i] + delta;
            }
            DrawShape(finalPoints, true, color, duration, depthTest);
            
            // Draw connections
            Debug.DrawLine(startingPoints[0], finalPoints[0], color, duration, depthTest);
            Debug.DrawLine(startingPoints[segments / 4], finalPoints[segments / 4], color, duration, depthTest);
            Debug.DrawLine(startingPoints[segments / 2], finalPoints[segments / 2], color, duration, depthTest);
            Debug.DrawLine(startingPoints[3 * segments / 4], finalPoints[3 * segments / 4], color, duration, depthTest);
        }
    }
}