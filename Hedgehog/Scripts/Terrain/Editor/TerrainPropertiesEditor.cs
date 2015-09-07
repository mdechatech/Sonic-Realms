using Hedgehog.Terrain;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Terrain.Editor
{
    [CustomEditor(typeof(TerrainProperties))]
    public class TerrainPropertiesEditor : UnityEditor.Editor
    {
        private TerrainProperties _instance;

        private static bool ShowAdvanced
        {
            get { return EditorPrefs.GetBool("TerrainPropertiesEditor.ShowAdvanced", false); }
            set { EditorPrefs.SetBool("TerrainPropertiesEditor.ShowAdvanced", value);}
        }

        private const TerrainSide LedgeSolidMask = TerrainSide.Bottom;
        private const int AppliesToChildrenLevel = int.MaxValue;

        public void OnEnable()
        {
            _instance = target as TerrainProperties;
        }

        public override void OnInspectorGUI()
        {
            if (_instance == null) return;

            if (ShowAdvanced)
            {
                EditorGUILayout.BeginHorizontal();
                _instance.AppliesToChildrenLevel = EditorGUILayout.IntField("Apply to Children",
                    _instance.AppliesToChildrenLevel);
                EditorGUILayout.PrefixLabel("levels down");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                var appliesToChildren = _instance.AppliesToChildrenLevel >= AppliesToChildrenLevel;
                appliesToChildren = EditorGUILayout.Toggle("Apply to Children", appliesToChildren);
                _instance.AppliesToChildrenLevel = appliesToChildren ? AppliesToChildrenLevel : 0;
            }
                
            EditorGUILayout.Space();

            _instance.MovingPlatform = EditorGUILayout.Toggle("Moving Platform", _instance.MovingPlatform);

            var ledgeSolid = _instance.SolidSides == LedgeSolidMask;
            ledgeSolid = EditorGUILayout.Toggle("Ledge", ledgeSolid);
            if (ledgeSolid) _instance.SolidSides = LedgeSolidMask;
            else _instance.SolidSides = TerrainSide.All;

            _instance.Friction = EditorGUILayout.FloatField("Friction", _instance.Friction);

            EditorGUILayout.Space();

            ShowAdvanced = EditorGUILayout.Toggle("Show Advanced", ShowAdvanced);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_instance);
            }
        }
    }
}
