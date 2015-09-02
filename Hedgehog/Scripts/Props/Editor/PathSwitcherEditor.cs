using Hedgehog.Utils;
using UnityEditor;

namespace Hedgehog.Props.Editor
{
    [CustomEditor(typeof(PathSwitcher))]
    public class PathSwitcherEditor : UnityEditor.Editor
    {
        private PathSwitcher _instance;
        private SerializedObject _serializedInstance;

        public void OnEnable()
        {
            _instance = target as PathSwitcher;
            _serializedInstance = new SerializedObject(_instance);
        }

        public override void OnInspectorGUI()
        {
            if (_instance == null) return;

            _instance.CollisionMode = HedgehogEditorGUIUtility.CollisionModeField(_instance.CollisionMode);
            if (_instance.CollisionMode == CollisionMode.Layers)
            {
                _instance.IfTerrainMaskHas = HedgehogEditorGUIUtility.LayerMaskField("If Terrain Mask Has",
                    _instance.IfTerrainMaskHas);
                _instance.AddLayers = HedgehogEditorGUIUtility.LayerMaskField("Then Add Layers", _instance.AddLayers);
                _instance.RemoveLayers = HedgehogEditorGUIUtility.LayerMaskField("And Remove Layers",
                    _instance.RemoveLayers);
            } else if (_instance.CollisionMode == CollisionMode.Tags)
            {
                HedgehogEditorGUIUtility.ReorderableListField("If Terrain Tags Has", _serializedInstance,
                    _serializedInstance.FindProperty("IfTerrainTagsHas"));
                HedgehogEditorGUIUtility.ReorderableListField("Then Add Tags", _serializedInstance,
                    _serializedInstance.FindProperty("AddTags"));
                HedgehogEditorGUIUtility.ReorderableListField("And Remove Tags", _serializedInstance,
                    _serializedInstance.FindProperty("RemoveTags"));
            } else if (_instance.CollisionMode == CollisionMode.Names)
            {
                HedgehogEditorGUIUtility.ReorderableListField("If Terrain Names Has", _serializedInstance,
                    _serializedInstance.FindProperty("IfTerrainNamesHas"));
                HedgehogEditorGUIUtility.ReorderableListField("Then Add Names", _serializedInstance,
                    _serializedInstance.FindProperty("AddNames"));
                HedgehogEditorGUIUtility.ReorderableListField("And Remove Names", _serializedInstance,
                    _serializedInstance.FindProperty("RemoveNames"));
            }
        }
    }
}
