using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public static class DebugUtility
    {
        private const int CircleSides = 10;
        
        public static void DrawCircle(Vector3 center, float radius, Color color = default(Color),
            float duration = 0, bool depthTest = true)
        {
            if (color == default(Color))
                color = Color.white;

            var lastPoint = center + Vector3.right*radius;

            for (var i = 1f/CircleSides; i < 360; ++i)
            {
                var point = center + new Vector3(Mathf.Cos(i*Mathf.Deg2Rad), Mathf.Sin(i*Mathf.Deg2Rad))*radius;
                Debug.DrawLine(lastPoint, point, color, duration, depthTest);
                lastPoint = point;
            }

            Debug.DrawLine(lastPoint, center + Vector3.right*radius, color, duration, depthTest);
        }

        // Angle is in degrees
        public static void DrawCrosshair(Vector3 center, float size, Color color = default(Color),
            float angle = 45, float duration = 0, bool depthTest = true)
        {
            if (color == default(Color))
                color = Color.white;

            Debug.DrawLine(center, center + (Vector3)DMath.RotateBy(Vector2.up, angle*Mathf.Deg2Rad)*size/2,
                color, duration, depthTest);
            Debug.DrawLine(center, center + (Vector3) DMath.RotateBy(Vector2.down, angle*Mathf.Deg2Rad)*size/2,
                color, duration, depthTest);
            Debug.DrawLine(center, center + (Vector3) DMath.RotateBy(Vector2.left, angle*Mathf.Deg2Rad)*size/2,
                color, duration, depthTest);
            Debug.DrawLine(center, center + (Vector3) DMath.RotateBy(Vector2.right, angle*Mathf.Deg2Rad)*size/2,
                color, duration, depthTest);
        }
    }
}
