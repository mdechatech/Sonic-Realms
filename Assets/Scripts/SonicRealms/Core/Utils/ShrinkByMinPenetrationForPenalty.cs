using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Hit detection can be fuzzy due to Physics2D.MinPenetrationForPenalty. At runtime, this
    /// component shrinks colliders by that amount to simulate pixel-perfect collision.
    /// </summary>
    [ExecuteInEditMode]
    public class ShrinkByMinPenetrationForPenalty : MonoBehaviour
    {
        protected void Reset()
        {
#if UNITY_EDITOR
            var collider = GetComponent<Collider2D>();
            if (!collider)
            {
                UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(this); };
                UnityEditor.EditorUtility.DisplayDialog("Invalid operation",
                    "This component can't be added unless the game object has a Collider2D component attached.",
                    "Ok");
            }
#endif
        }

        protected void Awake()
        {
            if (!Application.isPlaying)
                return;

            var collider = GetComponent<Collider2D>();
            if(!collider)
            {
                Destroy(this);
                return;
            }

            Shrink(GetComponent<Collider2D>());
        }

#if UNITY_EDITOR
        protected void Update()
        {
            if (!Application.isPlaying && !GetComponent<Collider2D>())
            {
                DestroyImmediate(this);
            }
        }
#endif

        public static void Shrink(Collider2D collider)
        {
            var boxCollider = collider as BoxCollider2D;
            if (boxCollider != null)
            {
                Shrink(boxCollider);
                return;
            }

            var circleCollider = collider as CircleCollider2D;
            if (circleCollider != null)
            {
                Shrink(circleCollider);
                return;
            }

            var polygonCollider = collider as PolygonCollider2D;
            if (polygonCollider != null)
            {
                Shrink(polygonCollider);
                return;
            }

            var edgeCollider = collider as EdgeCollider2D;
            if (edgeCollider != null)
            {
                Shrink(edgeCollider);
                return;
            }
        }

        public static void Shrink(BoxCollider2D collider)
        {
            collider.size = new Vector2(
                collider.size.x - Physics2D.defaultContactOffset*4,
                collider.size.y - Physics2D.defaultContactOffset*4);
        }

        public static void Shrink(CircleCollider2D collider)
        {
            collider.radius -= Physics2D.defaultContactOffset*2;
        }

        public static void Shrink(PolygonCollider2D collider)
        {
            var center = GetCenter(collider.points);
            var newPoints = new Vector2[collider.points.Length];

            for (var i = 0; i < newPoints.Length; ++i)
            {
                var point = collider.points[i];
                newPoints[i] = point - (point - center).normalized*Physics2D.defaultContactOffset*2;
            }

            collider.points = newPoints;
        }

        public static void Shrink(EdgeCollider2D collider)
        {
            var center = GetCenter(collider.points);
            var newPoints = new Vector2[collider.points.Length];

            for (var i = 0; i < newPoints.Length; ++i)
            {
                var point = collider.points[i];
                newPoints[i] = point - (point - center).normalized * Physics2D.defaultContactOffset*2;
            }
            
            collider.points = newPoints;
        }

        private static Vector2 GetCenter(Vector2[] points)
        {
            var total = Vector2.zero;

            for (var i = 0; i < points.Length; ++i)
            {
                total += points[i];
            }

            return total/points.Length;
        }
    }
}
