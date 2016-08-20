using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [InitializeOnLoad]
    public static class ColliderSnapper
    {
        // Menu paths
        private const string PixelSnapCollidersMenu = "Realms/Pixel Snap Colliders";
        private const string PixelSnapLevelObjectsMenu = "Realms/Pixel Snap Level Objects";

        // Default settings
        private const bool DefaultPixelSnapColliders = true;
        private const bool DefaultPixelSnapLevelObjects = true;
        private const float DefaultSnapInterval = 0.01f;
        
        // Timer stuff in case we don't wanna update every frame
        private const float UpdateRate = 1000;
        private const float UpdateTime = 1 / UpdateRate;
        private static float NextUpdateTime;

        // Cached settings
        private static bool _pixelSnapColliders;
        private static bool _pixelSnapLevelObjects;
        private static float _snapInterval;

        // Properties for settings, they just save to EditorPrefs when set
        public static bool PixelSnapColliders
        {
            get { return _pixelSnapColliders; }
            set
            {
                _pixelSnapColliders = value;
                EditorPrefs.SetBool("ColliderSnapper.PixelSnapColliders", value);
            }
        }

        public static bool PixelSnapLevelObjects
        {
            get { return _pixelSnapLevelObjects; }
            set
            {
                _pixelSnapLevelObjects = value;
                EditorPrefs.SetBool("ColliderSnapper.PixelSnapLevelObjects", value);
            }
        }

        public static float SnapInterval
        {
            get { return _snapInterval; }
            set
            {
                _snapInterval = value;
                EditorPrefs.SetFloat("ColliderSnapper.SnapInterval", value);
            }
        }

        static ColliderSnapper()
        {
            // Initialize cached settings to saved EditorPrefs
            _pixelSnapColliders = EditorPrefs.GetBool("ColliderSnapper.PixelSnapColliders", DefaultPixelSnapColliders);
            _pixelSnapLevelObjects = EditorPrefs.GetBool("ColliderSnapper.PixelSnapLevelObjects", DefaultPixelSnapLevelObjects);
            _snapInterval = EditorPrefs.GetFloat("ColliderSnapper.SnapInterval", DefaultSnapInterval);

            // Setup update timer
            EditorApplication.update += Update;
            ChangeNextUpdateTime();

            // Initialize the state of menu item checkboxes AFTER the menu is populated
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked(PixelSnapCollidersMenu, PixelSnapColliders);
                Menu.SetChecked(PixelSnapLevelObjectsMenu, PixelSnapLevelObjects);
            };
        }

        // Menu item functions

        [MenuItem(PixelSnapCollidersMenu, false, 100)]
        private static void TogglePixelSnapColliders()
        {
            PixelSnapColliders = !PixelSnapColliders;
            Menu.SetChecked(PixelSnapCollidersMenu, PixelSnapColliders);
        }

        [MenuItem(PixelSnapLevelObjectsMenu, false, 101)]
        private static void TogglePixelSnapLevelObjects()
        {
            PixelSnapLevelObjects = !PixelSnapLevelObjects;
            Menu.SetChecked(PixelSnapLevelObjectsMenu, PixelSnapLevelObjects);
        }

        private static void Update()
        {
            if (Application.isPlaying)
                return;

            // Make sure the update time is reasonable - if it's way ahead, that means realTimeSinceStartup
            // was reset to zero (usually due to a scene load) and we need to recalibrate
            if (Time.realtimeSinceStartup + UpdateTime + 0.001f < NextUpdateTime)
            {
                ChangeNextUpdateTime();
                return;
            }

            if (Time.realtimeSinceStartup < NextUpdateTime)
                return;

            ChangeNextUpdateTime();

            if (Selection.activeGameObject == null)
                return;

            if (PixelSnapColliders)
                DoPixelSnapColliders();

            if (PixelSnapLevelObjects)
                DoPixelSnapLevelObjects();
        }

        private static void ChangeNextUpdateTime()
        {
            NextUpdateTime = Time.realtimeSinceStartup + UpdateTime;
        }

        private static void DoPixelSnapColliders()
        {
            var collider = Selection.activeGameObject.GetComponent<Collider2D>();
            if (collider)
                SnapCollider(collider, _snapInterval);
        }

        private static void DoPixelSnapLevelObjects()
        {
            if (!IsLevelObject(Selection.activeGameObject))
                return;

            var transform = Selection.activeGameObject.transform;
            transform.position = new Vector3(
                DMath.Round(transform.position.x, _snapInterval),
                DMath.Round(transform.position.y, _snapInterval),
                transform.position.z);
        }
        
        // For now a game object is a level object if it's in one of the terrain layers, has a
        // trigger component, or player controller
        private static bool IsLevelObject(GameObject gameObject)
        {
            if (((1 << gameObject.layer) & CollisionLayers.AllMask) != 0)
                return true;

            if (gameObject.GetComponent<BaseTrigger>())
                return true;

            if (gameObject.GetComponent<HedgehogController>())
                return true;

            return false;
        }

        private static void SnapCollider(Collider2D collider, float interval = DefaultSnapInterval)
        {
            if (collider is BoxCollider2D)
                SnapBoxCollider((BoxCollider2D) collider, interval);

            if (collider is CircleCollider2D)
                SnapCircleCollider((CircleCollider2D) collider, interval);

            if (collider is EdgeCollider2D)
                SnapEdgeCollider((EdgeCollider2D) collider, interval);

            if (collider is PolygonCollider2D)
                SnapPolygonCollider((PolygonCollider2D) collider, interval);
        }

        // Helpers for snapping collider values

        private static void SnapBoxCollider(BoxCollider2D collider, float interval = DefaultSnapInterval)
        {
            collider.size = RoundPoint(collider.size, interval);
            collider.offset = RoundPoint(collider.offset, interval);
        }

        private static void SnapCircleCollider(CircleCollider2D collider, float interval = DefaultSnapInterval)
        {
            collider.radius = DMath.Round(collider.radius, interval);
            collider.offset = RoundPoint(collider.offset, interval);
        }

        private static void SnapEdgeCollider(EdgeCollider2D collider, float interval = DefaultSnapInterval)
        {
            collider.points = RoundPoints(collider.points, interval);
            collider.offset = RoundPoint(collider.offset, interval);
        }

        private static void SnapPolygonCollider(PolygonCollider2D collider, float interval = DefaultSnapInterval)
        {
            collider.points = RoundPoints(collider.points, interval);
            collider.offset = RoundPoint(collider.offset, interval);
        }

        // Helpers for rounding values

        private static Vector2[] RoundPoints(Vector2[] points, float interval = DefaultSnapInterval)
        {
            var result = new Vector2[points.Length];
            for (var i = 0; i < points.Length; ++i)
            {
                result[i] = RoundPoint(points[i]);
            }

            return result;
        }

        private static Vector2 RoundPoint(Vector2 point, float interval = DefaultSnapInterval)
        {
            return new Vector2(DMath.Round(point.x, interval), DMath.Round(point.y, interval));
        }
    }
}
