using SonicRealms.Core.Utils;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Actors.Editor
{
    [CustomEditor(typeof(HedgehogController))]
    public class HedgehogControllerEditor : UnityEditor.Editor
    {
        #region Instance Variables
        [SerializeField] private HedgehogController _instance;
        [SerializeField] private SerializedObject _serializedInstance;
        #endregion
        #region Foldout Variables
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

        protected static bool ShowPhysics
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowPhysics", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowPhysics", value); }
        }

        protected static bool ShowPerformance
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowPerformance", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowPerformance", value); }
        }

        protected static bool ShowAdvancedPhysics
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowAdvancedPhysics", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowAdvancedPhysics", value); }
        }

        protected static bool ShowAnimation
        {
            get { return EditorPrefs.GetBool("HedgehogControllerEditor.ShowAnimation", false); }
            set { EditorPrefs.SetBool("HedgehogControllerEditor.ShowAnimation", value); }
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

            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "RendererObject",
                "Animator");

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "CollisionMask");
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
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "MaxSpeed",
                    "GroundFriction",
                    "GravityDirection",
                    "SlopeGravity",
                    "AirGravity",
                    "AirDrag",
                    "DetachSpeed",
                    "MaxClimbAngle",
                    "LedgeClimbHeight",
                    "LedgeDropHeight"
                    );

                #region Advanced Physics
                ++EditorGUI.indentLevel;

                ShowAdvancedPhysics = EditorGUILayout.Foldout(ShowAdvancedPhysics, "Advanced");
                if (ShowAdvancedPhysics)
                {
                    ++EditorGUI.indentLevel;
                    HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                        "AirDragRequiredSpeed",
                        "AntiTunnelingSpeed",
                        "SlopeGravityBeginAngle"
                        );
                    --EditorGUI.indentLevel;
                }

                --EditorGUI.indentLevel;
                #endregion
            }
            #endregion
            ShowPerformance = EditorGUILayout.Foldout(ShowPerformance, "Performance");
            if (ShowPerformance)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "DisableNotifyPlatforms",
                    "DisableEvents",
                    "DisableCeilingCheck",
                    "DisableGroundCheck",
                    "DisableSideCheck");
            }
            #region Animation Foldout

            ShowAnimation = EditorGUILayout.Foldout(ShowAnimation, "Animation", foldoutStyle);
            if (ShowAnimation)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "AttachTrigger",
                    "DetachTrigger",
                    "AirSpeedXFloat",
                    "AirSpeedYFloat",
                    "GroundedBool",
                    "FacingForwardBool",
                    "GroundSpeedFloat",
                    "GroundSpeedBool",
                    "AbsGroundSpeedFloat",
                    "SurfaceAngleFloat");
            }
            #endregion
            #region Events Foldout
            ShowEvents = EditorGUILayout.Foldout(ShowEvents, "Events", foldoutStyle);
            if (ShowEvents)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "OnAttach", "OnDetach", "OnSteepDetach");
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

                EditorGUILayout.LabelField("Collision", headerStyle);

                HedgehogEditorGUIUtility.DrawProperties(serializedObject, 
                    "CollisionMask", "Reactives", "Grounded", "LeftWall", "LeftCeiling", "RightWall", "RightCeiling");
                EditorGUILayout.LabelField("Surface", headerStyle);
                GUI.enabled = Application.isPlaying && _instance.Grounded;
                EditorGUILayout.FloatField("Surface Angle", _instance.SurfaceAngle);
                EditorGUILayout.EnumPopup("Footing", _instance.Footing);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PrimarySurface"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SecondarySurface"));
                GUI.enabled = Application.isPlaying;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Velocity", headerStyle);
            
                _instance.Vx = EditorGUILayout.FloatField("X", _instance.Vx);
                _instance.Vy = EditorGUILayout.FloatField("Y", _instance.Vy);
                GUI.enabled = Application.isPlaying && _instance.Grounded;
                _instance.GroundVelocity = EditorGUILayout.FloatField("Ground", _instance.GroundVelocity);
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
