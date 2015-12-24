using Hedgehog.Core.Utils;
using UnityEditor;

namespace Hedgehog.Core.Editor
{
    public static class HedgehogInit
    {
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            var tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            var layers = tagManager.FindProperty("layers");
            layers.GetArrayElementAtIndex(CollisionLayers.Layer3).stringValue = CollisionLayers.Layer3Name;
            layers.GetArrayElementAtIndex(CollisionLayers.Layer2).stringValue = CollisionLayers.Layer2Name;
            layers.GetArrayElementAtIndex(CollisionLayers.Layer1).stringValue = CollisionLayers.Layer1Name;
            layers.GetArrayElementAtIndex(CollisionLayers.AlwaysCollide).stringValue = CollisionLayers.AlwaysCollideName;

            tagManager.ApplyModifiedProperties();
        }
    }
}
