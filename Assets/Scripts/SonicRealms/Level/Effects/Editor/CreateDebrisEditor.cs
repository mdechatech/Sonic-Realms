using System.Linq;
using SonicRealms.Core.Triggers.Editor;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Level.Effects.Editor
{
    [CustomEditor(typeof(CreateDebris))]
    [CanEditMultipleObjects]
    public class CreateDebrisEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ReactiveObjectEditor.DrawActivatorCheck(targets);

            if (targets.Count() > 1)
            {
                DrawDefaultInspector();
            }
            else
            {
                RealmsEditorUtility.DrawProperties(serializedObject, "DebrisObject", "DebrisSize", "UseSprite");
                var useSpriteProp = serializedObject.FindProperty("UseSprite");

                if (useSpriteProp.boolValue)
                {
                    RealmsEditorUtility.DrawProperties(serializedObject, "SpriteRenderer");
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
