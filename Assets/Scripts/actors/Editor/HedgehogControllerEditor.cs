using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.VR;

[Serializable]
[CustomEditor(typeof(HedgehogController))]
public class HedgehogControllerEditor : Editor
{
    [SerializeField] private HedgehogController _instance;

    [SerializeField]
    private Transform _generatedSensors;
    private const string GeneratedSensorsName = "__Generated Sensors__";

    [SerializeField]
    private bool _showCollision;
    [SerializeField]
    private bool _showSensors;
    [SerializeField]
    private bool _showSensorsGenerator;

    [SerializeField]
    private Renderer _fromRenderer;
    [SerializeField]
    private Collider2D _fromCollider2D;
    [SerializeField]
    private Bounds _fromBounds;

    [SerializeField]
    private bool _showAdvancedSensors;
    [SerializeField]
    private bool _showControls;
    [SerializeField]
    private bool _showPhysics;
    [SerializeField]
    private bool _showAdvancedPhysics;

    public void OnEnable()
    {
        _instance = target as HedgehogController;
        SearchGeneratedSensors();

        _fromRenderer = _instance.GetComponent<Renderer>();
        _fromCollider2D = _instance.GetComponent<Collider2D>();
    }

    private void SearchGeneratedSensors()
    {
        foreach (var child in _instance.transform)
        {
            var transform = child as Transform;
            if (transform.name == GeneratedSensorsName)
            {
                _generatedSensors = transform;
                return;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        if (_instance == null) return;

        var titleStyle = new GUIStyle
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            richText = true
        };

        var subtitleStyle = new GUIStyle
        {
            fontSize = 8,
            alignment = TextAnchor.MiddleCenter,
            richText = true
        };

        var headerStyle = new GUIStyle
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter,
        };

        var foldoutStyle = new GUIStyle(EditorStyles.foldout);

        var verticalStyle1 = new GUIStyle()
        {
            padding = new RectOffset(0, 0, 0, 0),
        };
        
        GUILayout.Label("<color=\"#0000ff\">Hedgehog Controller</color>", titleStyle);
        GUILayout.Label("<color=\"#5555ff\">mdechatech</color>", subtitleStyle);

        _showControls = EditorGUILayout.Foldout(_showControls, "Controls", foldoutStyle);
        if (_showControls)
        {
            _instance.JumpKey = (KeyCode)EditorGUILayout.EnumPopup("Jump Key", _instance.JumpKey);
            _instance.LeftKey = (KeyCode)EditorGUILayout.EnumPopup("Left Key", _instance.LeftKey);
            _instance.RightKey = (KeyCode)EditorGUILayout.EnumPopup("Right Key", _instance.RightKey);
        }

        _showCollision = EditorGUILayout.Foldout(_showCollision, "Collision", foldoutStyle);
        if (_showCollision)
        {
            _showSensorsGenerator = EditorGUILayout.Toggle("Show Sensors Creator", _showSensorsGenerator);
            if (_showSensorsGenerator)
            {
                EditorGUILayout.LabelField("Create Sensors", headerStyle);

                EditorGUILayout.BeginHorizontal();
                _fromRenderer =
                    EditorGUILayout.ObjectField("From Renderer", _fromRenderer, typeof(Renderer), true) as Renderer;
                GUI.enabled = _fromRenderer != null;
                if (GUILayout.Button("Create"))
                {
                    GenerateSensors(_fromRenderer.bounds);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _fromCollider2D =
                    EditorGUILayout.ObjectField("From Collider2D", _fromCollider2D, typeof(Collider2D), true) as Collider2D;
                GUI.enabled = _fromCollider2D != null;
                if (GUILayout.Button("Create"))
                {
                    GenerateSensors(_fromCollider2D.bounds);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                _fromBounds = EditorGUILayout.BoundsField("From Bounds", _fromBounds);
                GUI.enabled = _fromBounds != default(Bounds);
                if (GUILayout.Button("Create from Bounds"))
                {
                    GenerateSensors(_fromBounds, true);
                }
                GUI.enabled = true;
            }

            _showSensors = EditorGUILayout.Toggle("Show Sensors", _showSensors);
            if (_showSensors)
            {
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
        }

        _showPhysics = EditorGUILayout.Foldout(_showPhysics, "Physics", foldoutStyle);
        if (_showPhysics)
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

            _showAdvancedPhysics = EditorGUILayout.Toggle("Show Advanced", _showAdvancedPhysics);
            if (_showAdvancedPhysics)
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

                EditorGUILayout.LabelField("Surface Constants", headerStyle);

                EditorGUILayout.HelpBox("The following constants are best left untouched!", MessageType.Warning);

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
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_instance);
            EditorUtility.SetDirty(this);
        }
    }

    private void GenerateSensors(Bounds bounds, bool isLocal = false)
    {
        SearchGeneratedSensors();
        if (_generatedSensors != null)
        {
            DestroyImmediate(_generatedSensors.gameObject);
            _generatedSensors = null;
        }

        var sensorsObject = new GameObject();
        sensorsObject.name = GeneratedSensorsName;
        sensorsObject.transform.SetParent(_instance.transform);
        if (isLocal) sensorsObject.transform.localPosition = bounds.center;
        else sensorsObject.transform.position = bounds.center;

        var sensorTopLeft = new GameObject();
        sensorTopLeft.name = "Top Left";

        var sensorTopMid = new GameObject();
        sensorTopMid.name = "Top Mid";

        var sensorTopRight = new GameObject();
        sensorTopRight.name = "Top Right";

        var sensorMidLeft = new GameObject();
        sensorMidLeft.name = "Mid Left";

        var sensorMidMid = new GameObject();
        sensorMidMid.name = "Mid Mid";

        var sensorMidRight = new GameObject();
        sensorMidRight.name = "Mid Right";

        var sensorBotLeft = new GameObject();
        sensorBotLeft.name = "Bot Left";

        var sensorBotMid = new GameObject();
        sensorBotMid.name = "Bot Mid";

        var sensorBotRight = new GameObject();
        sensorBotRight.name = "Bot Right";

        sensorTopLeft.transform.SetParent(sensorsObject.transform);
        _instance.SensorTopLeft = sensorTopLeft.transform;

        sensorTopMid.transform.SetParent(sensorsObject.transform);
        _instance.SensorTopMiddle = sensorTopMid.transform;

        sensorTopRight.transform.SetParent(sensorsObject.transform);
        _instance.SensorTopRight = sensorTopRight.transform;

        sensorMidLeft.transform.SetParent(sensorsObject.transform);
        _instance.SensorMiddleLeft = sensorMidLeft.transform;

        sensorMidMid.transform.SetParent(sensorsObject.transform);
        _instance.SensorMiddleMiddle = sensorMidMid.transform;

        sensorMidRight.transform.SetParent(sensorsObject.transform);
        _instance.SensorMiddleRight = sensorMidRight.transform;

        sensorBotLeft.transform.SetParent(sensorsObject.transform);
        _instance.SensorBottomLeft = sensorBotLeft.transform;

        sensorBotMid.transform.SetParent(sensorsObject.transform);
        _instance.SensorBottomMiddle = sensorBotMid.transform;

        sensorBotRight.transform.SetParent(sensorsObject.transform);
        _instance.SensorBottomRight = sensorBotRight.transform;

        if (isLocal)
        {
            sensorTopLeft.transform.localPosition = new Vector3(bounds.min.x, bounds.max.y);
            sensorTopMid.transform.localPosition = new Vector3(bounds.center.x, bounds.max.y);
            sensorTopRight.transform.localPosition = bounds.max;
            sensorMidLeft.transform.localPosition = new Vector3(bounds.min.x - 0.01f, bounds.center.y);
            sensorMidMid.transform.localPosition = bounds.center;
            sensorMidRight.transform.localPosition = new Vector3(bounds.max.x + 0.01f, bounds.center.y);
            sensorBotLeft.transform.localPosition = bounds.min;
            sensorBotMid.transform.localPosition = new Vector3(bounds.center.x, bounds.min.y);
            sensorBotRight.transform.localPosition = new Vector3(bounds.max.x, bounds.min.y);
        }
        else
        {
            sensorTopLeft.transform.position = new Vector3(bounds.min.x, bounds.max.y);
            sensorTopMid.transform.position = new Vector3(bounds.center.x, bounds.max.y);
            sensorTopRight.transform.position = bounds.max;
            sensorMidLeft.transform.position = new Vector3(bounds.min.x - 0.01f, bounds.center.y);
            sensorMidMid.transform.position = bounds.center;
            sensorMidRight.transform.position = new Vector3(bounds.max.x + 0.01f, bounds.center.y);
            sensorBotLeft.transform.position = bounds.min;
            sensorBotMid.transform.position = new Vector3(bounds.center.x, bounds.min.y);
            sensorBotRight.transform.position = new Vector3(bounds.max.x, bounds.min.y);
        }

        _instance.SensorTopLeft = sensorTopLeft.transform;
        _instance.SensorTopMiddle = sensorTopMid.transform;
        _instance.SensorTopRight = sensorTopRight.transform;
        _instance.SensorMiddleLeft = sensorMidLeft.transform;
        _instance.SensorMiddleMiddle = sensorMidMid.transform;
        _instance.SensorMiddleRight = sensorMidRight.transform;
        _instance.SensorBottomLeft = sensorBotLeft.transform;
        _instance.SensorBottomMiddle = sensorBotMid.transform;
        _instance.SensorBottomRight = sensorBotRight.transform;
    }
}
