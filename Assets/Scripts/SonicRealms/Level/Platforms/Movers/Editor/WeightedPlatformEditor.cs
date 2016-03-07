using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Level.Platforms.Movers.Editor
{
    [CustomEditor(typeof(WeightedPlatform))]
    public class WeightedPlatformEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "ReverseDirection",
                "PingPong");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
