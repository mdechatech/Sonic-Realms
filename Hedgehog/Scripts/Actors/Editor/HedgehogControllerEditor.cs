using System;
using System.Collections.Generic;
using Hedgehog.Actors;
using Hedgehog.Utils;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Editor
{
    [CustomEditor(typeof(HedgehogController))]
    public class HedgehogControllerEditor : UnityEditor.Editor
    {
        #region Instance Variables
        [SerializeField] private HedgehogController _instance;
        [SerializeField] private SerializedObject _serializedInstance;
        #endregion
        #region Foldout Variables
        private static bool ShowCollision
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowCollision", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowCollision", value); }
        }

        private static bool ShowSensors
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowSensors", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowSensors", value);}
        }

        private bool ShowSensorsGenerator
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowSensorsGenerator", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowSensorsGenerator", value); }
        }

        private bool ShowAdvancedSensors
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowAdvancedSensors", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowAdvancedSensors", value); }
        }

        private static bool ShowControls
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowControls", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowControls", value); }
        }

        private static bool ShowPhysics
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowPhysics", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowPhysics", value); }
        }

        private static bool ShowAdvancedPhysics
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowAdvancedPhysics", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowAdvancedPhysics", value); }
        }

        private bool ShowDebugInfo
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowDebugInfo", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowDebugInfo", value); }
        }
        #endregion
        #region Sensor Creator Variables
        [SerializeField]
        private Renderer _fromRenderer;

        [SerializeField]
        private Collider2D _fromCollider2D;

        [SerializeField]
        private Bounds _fromBounds;
        #endregion

        public void OnEnable()
        {
            _instance = target as HedgehogController;
            _serializedInstance = new SerializedObject(_instance);

            _fromRenderer = _instance.GetComponent<Renderer>();
            _fromCollider2D = _instance.GetComponent<Collider2D>();
        }

        public override void OnInspectorGUI()
        {
            if (_instance == null) return;

            #region GUI Styles
            var titleStyle = new GUIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            var headerStyle = new GUIStyle
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
            };

            var foldoutStyle = new GUIStyle(EditorStyles.foldout);
            #endregion

            #region Title
            GUILayout.Label("<color=\"#0000ff\">Hedgehog Controller</color>", titleStyle);
            #endregion

            #region Controls Foldout
            ShowControls = EditorGUILayout.Foldout(ShowControls, "Controls", foldoutStyle);
            if (ShowControls)
            {
                _instance.JumpKey = (KeyCode)EditorGUILayout.EnumPopup("Jump Key", _instance.JumpKey);
                _instance.LeftKey = (KeyCode)EditorGUILayout.EnumPopup("Left Key", _instance.LeftKey);
                _instance.RightKey = (KeyCode)EditorGUILayout.EnumPopup("Right Key", _instance.RightKey);
                _instance.DebugSpindashKey =
                    (KeyCode) EditorGUILayout.EnumPopup("Debug Spindash Key", _instance.DebugSpindashKey);
            }
            #endregion
            #region Collision Foldout
            ShowCollision = EditorGUILayout.Foldout(ShowCollision, "Collision", foldoutStyle);
            if (ShowCollision)
            {
                _instance.CollisionMode = HedgehogEditorGUIUtility.CollisionModeField(_instance.CollisionMode);
                if (_instance.CollisionMode == CollisionMode.Layers)
                {
                    _instance.InitialTerrainMask = 
                        HedgehogEditorGUIUtility.LayerMaskField("Terrain Layer Mask", _instance.InitialTerrainMask);
                } else if (_instance.CollisionMode == CollisionMode.Tags)
                {
                    HedgehogEditorGUIUtility.ReorderableListField("Collide with Tags",
                        _serializedInstance, _serializedInstance.FindProperty("TerrainTags"));
                } else if (_instance.CollisionMode == CollisionMode.Names)
                {
                    HedgehogEditorGUIUtility.ReorderableListField("Collide with Names",
                        _serializedInstance, _serializedInstance.FindProperty("TerrainNames"));
                }
            }
            #endregion
            #region Sensors Foldout
            ShowSensors = EditorGUILayout.Foldout(ShowSensors, "Sensors", foldoutStyle);
            if (ShowSensors)
            {
                EditorGUILayout.LabelField("Create", headerStyle);

                EditorGUILayout.BeginHorizontal();
                _fromRenderer =
                    EditorGUILayout.ObjectField("From Renderer", _fromRenderer, typeof(Renderer), true) as Renderer;
                GUI.enabled = _fromRenderer != null && _fromRenderer.bounds != default(Bounds);
                if (GUILayout.Button("Create"))
                {
                    HedgehogUtility.GenerateSensors(_instance, _fromRenderer.bounds);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _fromCollider2D =
                    EditorGUILayout.ObjectField("From Collider2D", _fromCollider2D, typeof(Collider2D), true) as Collider2D;
                GUI.enabled = _fromCollider2D != null;
                if (GUILayout.Button("Create"))
                {
                    HedgehogUtility.GenerateSensors(_instance, _fromCollider2D.bounds);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                _fromBounds = EditorGUILayout.BoundsField("From Bounds", _fromBounds);
                GUI.enabled = _fromBounds != default(Bounds);
                if (GUILayout.Button("Create from Bounds"))
                {
                    HedgehogUtility.GenerateSensors(_instance, _fromBounds, true);
                }
                GUI.enabled = true;

                EditorGUILayout.Space();
            
                _instance.SensorTopLeft = EditorGUILayout.ObjectField("Top Left", _instance.SensorTopLeft,
                    typeof(Transform), true) as Transform;
                _instance.SensorTopMiddle = EditorGUILayout.ObjectField("Top Center", _instance.SensorTopMiddle,
                    typeof(Transform), true) as Transform;
                _instance.SensorTopRight = EditorGUILayout.ObjectField("Top Right", _instance.SensorTopRight,
                    typeof(Transform), true) as Transform;
                _instance.SensorMiddleLeft = EditorGUILayout.ObjectField("Center Left", _instance.SensorMiddleLeft,
                    typeof(Transform), true) as Transform;
                _instance.SensorMiddleMiddle = EditorGUILayout.ObjectField("Center", _instance.SensorMiddleMiddle,
                    typeof(Transform), true) as Transform;
                _instance.SensorMiddleRight = EditorGUILayout.ObjectField("Center Right", _instance.SensorMiddleRight,
                    typeof(Transform), true) as Transform;
                _instance.SensorBottomLeft = EditorGUILayout.ObjectField("Bottom Left", _instance.SensorBottomLeft,
                    typeof(Transform), true) as Transform;
                _instance.SensorBottomMiddle = EditorGUILayout.ObjectField("Bottom Center", _instance.SensorBottomMiddle,
                    typeof(Transform), true) as Transform;
                _instance.SensorBottomRight = EditorGUILayout.ObjectField("Bottom Right", _instance.SensorBottomRight,
                    typeof(Transform), true) as Transform;
            }
            #endregion
            #region Physics Foldout
            ShowPhysics = EditorGUILayout.Foldout(ShowPhysics, "Physics", foldoutStyle);
            if (ShowPhysics)
            {
                EditorGUILayout.BeginHorizontal();
                _instance.MaxSpeed = EditorGUILayout.FloatField("Max Speed",
                    _instance.MaxSpeed);
                EditorGUILayout.PrefixLabel("units/s");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _instance.JumpSpeed = EditorGUILayout.FloatField("Jump Speed",
                    _instance.JumpSpeed);
                EditorGUILayout.PrefixLabel("units/s");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _instance.ReleaseJumpSpeed = EditorGUILayout.FloatField("Jump Release Speed",
                    _instance.ReleaseJumpSpeed);
                EditorGUILayout.PrefixLabel("units/s");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                _instance.GroundAcceleration = EditorGUILayout.FloatField("Ground Acceleration",
                    _instance.GroundAcceleration);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _instance.GroundDeceleration = EditorGUILayout.FloatField("Ground Deceleration",
                    _instance.GroundDeceleration);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _instance.AirAcceleration = EditorGUILayout.FloatField("Air Acceleration",
                    _instance.AirAcceleration);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                _instance.AirGravity = EditorGUILayout.FloatField("Air Gravity",
                    _instance.AirGravity);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _instance.SlopeGravity = EditorGUILayout.FloatField("Slope Gravity",
                    _instance.SlopeGravity);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                #region Advanced Physics
                ShowAdvancedPhysics = EditorGUILayout.Toggle("Show Advanced", ShowAdvancedPhysics);
                if (ShowAdvancedPhysics)
                {
                    EditorGUILayout.BeginHorizontal();
                    _instance.HorizontalLockTime = EditorGUILayout.FloatField("Horizontal Lock",
                        _instance.HorizontalLockTime);
                    EditorGUILayout.PrefixLabel("seconds");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    _instance.LedgeClimbHeight = EditorGUILayout.FloatField("Ledge Climb Height",
                        _instance.LedgeClimbHeight);
                    EditorGUILayout.PrefixLabel("units");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    _instance.LedgeDropHeight = EditorGUILayout.FloatField("Ledge Drop Height",
                        _instance.LedgeDropHeight);
                    EditorGUILayout.PrefixLabel("units");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    _instance.SlopeGravityBeginAngle = EditorGUILayout.FloatField("Slope Gravity Begin Angle",
                        _instance.SlopeGravityBeginAngle);
                    EditorGUILayout.PrefixLabel("degrees");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    _instance.ForceJumpAngleDifference = EditorGUILayout.FloatField("Force Jump Angle",
                        _instance.ForceJumpAngleDifference);
                    EditorGUILayout.PrefixLabel("degrees");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    _instance.AntiTunnelingSpeed = EditorGUILayout.FloatField("Anti-Tunneling Speed",
                        _instance.AntiTunnelingSpeed);
                    EditorGUILayout.PrefixLabel("units/s");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    _instance.DetachSpeed = EditorGUILayout.FloatField("Detach Speed",
                        _instance.DetachSpeed);
                    EditorGUILayout.PrefixLabel("units/s");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    #region Forbidden Constants
                    EditorGUILayout.LabelField("Surface Values", headerStyle);
                    EditorGUILayout.HelpBox("The following values are best left untouched!", MessageType.Warning);

                    EditorGUILayout.BeginHorizontal();
                    _instance.MinWallmodeSwitchSpeed = EditorGUILayout.FloatField("Min Wallmode Switch Speed",
                        _instance.MinWallmodeSwitchSpeed);
                    EditorGUILayout.PrefixLabel("units/s");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    _instance.HorizontalWallmodeAngleWeight = EditorGUILayout.FloatField("Horizontal Wallmode Angle Weight",
                        _instance.HorizontalWallmodeAngleWeight);
                    EditorGUILayout.PrefixLabel("degrees");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    _instance.MaxSurfaceAngleDifference = EditorGUILayout.FloatField("Max Surface Angle Difference",
                        _instance.MaxSurfaceAngleDifference);
                    EditorGUILayout.PrefixLabel("degrees");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    _instance.MinOverlapAngle = EditorGUILayout.FloatField("Min Overlap Angle",
                        _instance.MinOverlapAngle);
                    EditorGUILayout.PrefixLabel("degrees");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    _instance.MinFlatOverlapRange = EditorGUILayout.FloatField("Min Flat Overlap Range",
                        _instance.MinFlatOverlapRange);
                    EditorGUILayout.PrefixLabel("degrees");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    _instance.MinFlatAttachAngle = EditorGUILayout.FloatField("Min Flat Attach Angle",
                        _instance.MinFlatAttachAngle);
                    EditorGUILayout.PrefixLabel("degrees");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    _instance.MaxVerticalDetachAngle = EditorGUILayout.FloatField("Max Vertical Detach Angle",
                        _instance.MaxVerticalDetachAngle);
                    EditorGUILayout.PrefixLabel("degrees");
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }
                #endregion
            }
            #endregion
            #region Debug Foldout
            ShowDebugInfo = EditorGUILayout.Foldout(ShowDebugInfo, "Debug", foldoutStyle);
            if (ShowDebugInfo)
            {
                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("This section becomes active in Play Mode.", MessageType.Info);
                }

                GUI.enabled = Application.isPlaying;

                EditorGUILayout.LabelField("Control", headerStyle);

                EditorGUILayout.Toggle("Jump Key Pressed", _instance.JumpKeyDown);
                EditorGUILayout.Toggle("Left Key Pressed", _instance.LeftKeyDown);
                EditorGUILayout.Toggle("Right Key Pressed", _instance.RightKeyDown);

                EditorGUILayout.Space();

                EditorGUILayout.Toggle("Lock Upon Landing", _instance.LockUponLanding);
                EditorGUILayout.Toggle("Horizontal Lock", _instance.HorizontalLock);
                GUI.enabled = Application.isPlaying && _instance.HorizontalLock;
                EditorGUILayout.FloatField("Countdown", _instance.HorizontalLockTimer);
                GUI.enabled = Application.isPlaying;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Collision", headerStyle);

                _instance.CollisionMode =
                    (CollisionMode) EditorGUILayout.EnumPopup("Collision Mode", _instance.CollisionMode);

                if (_instance.CollisionMode == CollisionMode.Layers)
                {
                    _instance.TerrainMask = HedgehogEditorGUIUtility.LayerMaskField("Terrain Mask",
                        _instance.TerrainMask);
                } else if (_instance.CollisionMode == CollisionMode.Tags)
                {
                    HedgehogEditorGUIUtility.ReorderableListField("Terrain Tags", _serializedInstance,
                        _serializedInstance.FindProperty("TerrainTags"));
                } else if (_instance.CollisionMode == CollisionMode.Names)
                {
                    HedgehogEditorGUIUtility.ReorderableListField("Terrain Names", _serializedInstance,
                        _serializedInstance.FindProperty("TerrainNames"));
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Surface", headerStyle);
                GUI.enabled = Application.isPlaying && _instance.Grounded;
                EditorGUILayout.FloatField("Surface Angle", _instance.SurfaceAngle);
                EditorGUILayout.EnumPopup("Wallmode", _instance.Wallmode);
                GUI.enabled = Application.isPlaying;
                _instance.Grounded = EditorGUILayout.Toggle("Grounded", _instance.Grounded);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Velocity", headerStyle);
            
                _instance.Vx = EditorGUILayout.FloatField("X", _instance.Vx);
                _instance.Vy = EditorGUILayout.FloatField("Y", _instance.Vy);
                GUI.enabled = Application.isPlaying && _instance.Grounded;
                _instance.Vg = EditorGUILayout.FloatField("Ground", _instance.Vg);
                GUI.enabled = Application.isPlaying;

                GUI.enabled = true;
            }
            #endregion

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_instance);
                EditorUtility.SetDirty(this);
            }
        }
    }
}
