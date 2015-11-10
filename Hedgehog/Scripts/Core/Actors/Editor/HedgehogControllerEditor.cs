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
                    "GroundControl", "AirControl", "AutoFindMoves", "Moves");
            }
            #endregion
            #region Collision Foldout
            ShowCollision = EditorGUILayout.Foldout(ShowCollision, "Collision", foldoutStyle);
            if (ShowCollision)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "Paths");
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
            #region Events Foldout
            ShowEvents = EditorGUILayout.Foldout(ShowEvents, "Events", foldoutStyle);
            if (ShowEvents)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "OnCrush", "OnAttach",
                    "OnDetach", "OnSteepDetach", "OnPerformMove", "OnInterruptedMove");
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

                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "Paths");

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
