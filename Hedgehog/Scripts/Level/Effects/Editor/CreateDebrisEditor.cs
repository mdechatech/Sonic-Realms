using System.Linq;
using Hedgehog.Core.Utils.Editor;
using Hedgehog.Level.Effects;
using UnityEditor;

namespace Hedgehog.Level.Effects.Editor
{
    [CustomEditor(typeof(CreateDebris))]
    [CanEditMultipleObjects]
    public class CreateDebrisEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (targets.Count() > 1)
            {
                DrawDefaultInspector();
            }
            else
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "DebrisObject", "DebrisSize", "UseSprite");
                var useSpriteProp = serializedObject.FindProperty("UseSprite");

                if (useSpriteProp.boolValue)
                {
                    HedgehogEditorGUIUtility.DrawProperties(serializedObject, "SpriteRenderer");
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
