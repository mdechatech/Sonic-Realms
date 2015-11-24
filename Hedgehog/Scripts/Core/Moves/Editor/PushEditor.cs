using Hedgehog.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(Push))]
    public class PushEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            var useControlInput = serializedObject.FindProperty("UseControlInput");
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "UseControlInput");

            serializedObject.ApplyModifiedProperties();

            var enabled = GUI.enabled;
            GUI.enabled = !useControlInput.boolValue;
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ActivateAxis");
            GUI.enabled = enabled;

            base.DrawControlProperties();
        }
    }
}
