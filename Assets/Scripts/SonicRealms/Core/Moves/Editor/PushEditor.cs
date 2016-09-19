using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Moves.Editor
{
    [CustomEditor(typeof(Push))]
    public class PushEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            var useControlInput = serializedObject.FindProperty("UseControlInput");
            RealmsEditorUtility.DrawProperties(serializedObject, "UseControlInput");

            serializedObject.ApplyModifiedProperties();

            var enabled = GUI.enabled;
            GUI.enabled = !useControlInput.boolValue;
            RealmsEditorUtility.DrawProperties(serializedObject, "ActivateAxis");
            GUI.enabled = enabled;
        }
    }
}
