using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Utils;
using Hedgehog.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Core.Actors.Editor
{
    [CustomEditor(typeof(HedgehogController))]
    public class HedgehogControllerEditor : UnityEditor.Editor
    {
        #region Instance Variables
        [SerializeField] private HedgehogController _instance;
        [SerializeField] private SerializedObject _serializedInstance;
        #endregion
        #region Foldout Variables
        protected static bool ShowMoves
        {
            get { return EditorPrefs.GetBool("HedgehogController.ShowMoves", false); }
            set { EditorPrefs.SetBool("HedgehogController.ShowMoves", value); }
        }

        protected static bool ShowCollision
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowCollision", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowCollision", value); }
        }

        protected static bool ShowSensors
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowSensors", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowSensors", value);}
        }

        protected bool ShowSensorsGenerator
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowSensorsGenerator", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowSensorsGenerator", value); }
        }

        protected bool ShowAdvancedSensors
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowAdvancedSensors", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowAdvancedSensors", value); }
        }

        protected static bool ShowPhysics
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowPhysics", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowPhysics", value); }
        }

        protected static bool ShowPhysicsPresets
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowPhysicsPresets", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowPhysicsPresets", value); }
        }

        protected static bool ShowAdvancedPhysics
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowAdvancedPhysics", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowAdvancedPhysics", value); }
        }

        protected static bool ShowAdvancedPhysicsSpeeds
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowAdvancedPhysicsSpeeds", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowAdvancedPhysicsSpeeds", value); }
        }

        protected static bool ShowAdvancedPhysicsSurfaces
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowAdvancedPhysicsSurfaces", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowAdvancedPhysicsSurfaces", value); }
        }

        protected static bool ShowEvents
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowEvents", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowEvents", value); }
        }

        protected bool ShowDebugInfo
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowDebugInfo", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowDebugInfo", value); }
        }
        #endregion
        #region Physics Preset Variables
        private static readonly IDictionary<string, HedgehogPhysicsValues> PhysicsPresets =
            new Dictionary<string, HedgehogPhysicsValues>
            {
                {"Genesis Era/Sonic and Tails", HedgehogUtility.SonicPhysicsValues},
                {"Genesis Era/Knuckles", HedgehogUtility.KnucklesPhysicsValues}
            };

        private static readonly string[] PhysicsPresetResolutionSources = new string[]
        {
            "Player vs. Camera Size",
            "Camera Size",
            "Orthographic Size",
            "Screen Size",
        };

        private int _physicsPresetIndex = 0;
        private float _physicsPresetOrthographicSize = 5.0f;
        private Vector2 _physicsPresetScreenSize;
        private int _physicsPresetResolutionSourceIndex = 0;
        private Camera _physicsPresetCamera;
        private Vector2 _physicsPresetPlayerSize;
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
            serializedObject.Update();

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

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "RendererObject", "Animator");

            #region Moves Foldout
            ShowMoves = EditorGUILayout.Foldout(ShowMoves, "Moves", foldoutStyle);
            if (ShowMoves)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "DefaultGroundState", "DefaultAirState", "AutoFindMoves", "Moves");
            }
            #endregion
            #region Collision Foldout
            ShowCollision = EditorGUILayout.Foldout(ShowCollision, "Collision", foldoutStyle);
            if (ShowCollision)
            {
                _instance.CollisionMode = HedgehogEditorGUIUtility.CollisionModeField(_instance.CollisionMode);
                if (_instance.CollisionMode == CollisionMode.Layers)
                {
                    _instance.TerrainMask = 
                        HedgehogEditorGUIUtility.LayerMaskField("Terrain Layer Mask", _instance.TerrainMask);
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

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Sensors"));
            }
            #endregion
            #region Physics Foldout
            ShowPhysics = EditorGUILayout.Foldout(ShowPhysics, "Physics", foldoutStyle);
            if (ShowPhysics)
            {
                #region Generator
                ++EditorGUI.indentLevel;
                ShowPhysicsPresets = EditorGUILayout.Foldout(ShowPhysicsPresets, "Presets", foldoutStyle);
                if (ShowPhysicsPresets)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Preset");
                    --EditorGUI.indentLevel;
                    _physicsPresetIndex = EditorGUILayout.Popup(_physicsPresetIndex, PhysicsPresets.Keys.ToArray());
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Use Resolution From");
                    --EditorGUI.indentLevel;
                    _physicsPresetResolutionSourceIndex =
                        EditorGUILayout.Popup(_physicsPresetResolutionSourceIndex, PhysicsPresetResolutionSources);
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.EndHorizontal();

                    var resolutionSource = PhysicsPresetResolutionSources[_physicsPresetResolutionSourceIndex];

                    switch (resolutionSource)
                    {
                        case "Camera Size":
                            if (_physicsPresetCamera == null && Camera.main != null)
                            {
                                _physicsPresetCamera = Camera.main;
                            }

                            _physicsPresetCamera = EditorGUILayout.ObjectField("Camera", _physicsPresetCamera,
                            typeof(Camera), true) as Camera;

                            if (_physicsPresetCamera.orthographic)
                            {
                                _physicsPresetOrthographicSize = _physicsPresetCamera.orthographicSize;
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Using perspective projection is inaccurate. Try finding a similar orthographic size. You can switch back when you're done.",
                                    MessageType.Warning);
                                _physicsPresetOrthographicSize =
                                    HedgehogUtility.FOV2OrthographicSize(_physicsPresetOrthographicSize);
                            }
                            break;

                        case "Player vs. Camera Size":
                            if (_physicsPresetOrthographicSize == default(float))
                            {
                                var camera = Camera.main;
                                if (camera != null)
                                    if (camera.orthographic)
                                        _physicsPresetOrthographicSize = Camera.main.orthographicSize;
                                    else
                                        _physicsPresetOrthographicSize =
                                            HedgehogUtility.FOV2OrthographicSize(_physicsPresetOrthographicSize);
                            }

                            _physicsPresetOrthographicSize = EditorGUILayout.FloatField("Camera Orthographic Size",
                                _physicsPresetOrthographicSize);

                            if (_physicsPresetPlayerSize == default(Vector2))
                            {
                                var sprite = _instance.GetComponent<SpriteRenderer>();
                                if (sprite != null)
                                    _physicsPresetPlayerSize = sprite.bounds.size;
                            }

                            _physicsPresetPlayerSize = EditorGUILayout.Vector2Field("Player Size (units)",
                                _physicsPresetPlayerSize);
                            break;

                        case "Screen Size":
                            _physicsPresetScreenSize = EditorGUILayout.Vector2Field("Screen Size (pixels)",
                                _physicsPresetScreenSize);

                            _physicsPresetOrthographicSize = _physicsPresetScreenSize.y/200.0f;
                            break;

                        case "Orthographic Size":
                            if (_physicsPresetOrthographicSize == default(float))
                            {
                                var camera = Camera.main;
                                if (camera != null)
                                    if (camera.orthographic)
                                        _physicsPresetOrthographicSize = Camera.main.orthographicSize;
                                    else
                                        _physicsPresetOrthographicSize =
                                            HedgehogUtility.FOV2OrthographicSize(_physicsPresetOrthographicSize);
                            }

                            _physicsPresetOrthographicSize = EditorGUILayout.FloatField("Orthographic Size",
                                _physicsPresetOrthographicSize);
                            break;
                    }

                    var physicsPreset = PhysicsPresets[PhysicsPresets.Keys.ToArray()[_physicsPresetIndex]];

                    if (GUILayout.Button("Apply Preset"))
                    {
                        if (EditorUtility.DisplayDialog("Apply Preset", "Are you sure? Please back up your " +
                                                                        "current values first!", "Yes", "No"))
                        {
                            if (resolutionSource == "Player vs. Camera Size")
                                (physicsPreset*(_physicsPresetPlayerSize.y/_physicsPresetOrthographicSize/
                                                HedgehogUtility.MegadrivePlayerCameraRatio)).Apply(_instance);
                            else
                                (physicsPreset*(_physicsPresetOrthographicSize/physicsPreset.TargetOrthographicSize
                                    .Value)).Apply(_instance);
                        }
                    }
                }
                --EditorGUI.indentLevel;
                #endregion
                EditorGUILayout.BeginHorizontal();
                _instance.MaxSpeed = EditorGUILayout.FloatField("Max Speed",
                    _instance.MaxSpeed);
                EditorGUILayout.PrefixLabel("units/s");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                _instance.GroundFriction = EditorGUILayout.FloatField("Ground Deceleration",
                    _instance.GroundFriction);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _instance.SlopeGravity = EditorGUILayout.FloatField("Slope Gravity",
                    _instance.SlopeGravity);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                _instance.AirAcceleration = EditorGUILayout.FloatField("Air Acceleration",
                    _instance.AirAcceleration);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _instance.AirDragCoefficient = EditorGUILayout.FloatField("Air Drag",
                    _instance.AirDragCoefficient);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _instance.AirGravity = EditorGUILayout.FloatField("Air Gravity",
                    _instance.AirGravity);
                EditorGUILayout.PrefixLabel("units/s²");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("GravityDirection"));
                EditorGUILayout.PrefixLabel("degrees");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                #region Advanced Physics
                ++EditorGUI.indentLevel;

                ShowAdvancedPhysics = EditorGUILayout.Foldout(ShowAdvancedPhysics, "Advanced");
                if (ShowAdvancedPhysics)
                {
                    ++EditorGUI.indentLevel;
                    #region Speed & Control
                    ShowAdvancedPhysicsSpeeds = EditorGUILayout.Foldout(ShowAdvancedPhysicsSpeeds, "Speed & Control");
                    if (ShowAdvancedPhysicsSpeeds)
                    {
                        Vector2 airDragVelocity =
                        EditorGUILayout.Vector2Field("Min Air Drag Velocity",
                            new Vector2(_instance.AirDragHorizontalSpeed, _instance.AirDragVerticalSpeed));
                        _instance.AirDragHorizontalSpeed = airDragVelocity.x;
                        _instance.AirDragVerticalSpeed = airDragVelocity.y;

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

                        EditorGUILayout.BeginHorizontal();
                        _instance.SlopeGravityBeginAngle = EditorGUILayout.FloatField("Slope Gravity Begin Angle",
                            _instance.SlopeGravityBeginAngle);
                        EditorGUILayout.PrefixLabel("degrees");
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion
                    #region Surfaces
                    ShowAdvancedPhysicsSurfaces = EditorGUILayout.Foldout(ShowAdvancedPhysicsSurfaces, "Surfaces");
                    if (ShowAdvancedPhysicsSurfaces)
                    {
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

                        EditorGUILayout.BeginHorizontal();
                        _instance.MaxSurfaceAngleDifference = EditorGUILayout.FloatField("Max Surface Angle Difference",
                            _instance.MaxSurfaceAngleDifference);
                        EditorGUILayout.PrefixLabel("degrees");
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        _instance.MaxVerticalDetachAngle = EditorGUILayout.FloatField("Max Vertical Detach Angle",
                            _instance.MaxVerticalDetachAngle);
                        EditorGUILayout.PrefixLabel("degrees");
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion
                    --EditorGUI.indentLevel;
                }

                --EditorGUI.indentLevel;
                #endregion
            }
            #endregion
            #region Events Foldout
            ShowEvents = EditorGUILayout.Foldout(ShowEvents, "Events", foldoutStyle);
            if (ShowEvents)
            {
                HedgehogEditorGUIUtility.UnityEventField(_instance.OnCrush,
                    _serializedInstance.FindProperty("OnCrush"));
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

                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ControlState", "ActiveMoves",
                    "AvailableMoves", "UnavailableMoves");

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
                EditorGUILayout.EnumPopup("Footing", _instance.Footing);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PrimarySurface"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SecondarySurface"));
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
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_instance);
                _serializedInstance.ApplyModifiedProperties();
                EditorUtility.SetDirty(this);
            }
        }
    }
}
