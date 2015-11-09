using System.Linq;
using Hedgehog.Core.Utils.Editor;
using Hedgehog.Level.Effects;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Level.Effects.Editor
{
    [CustomEditor(typeof(SwitchPath))]
    [CanEditMultipleObjects]
    public class SwitchPathEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (targets.Count() <= 1)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "MustBeGrounded");

                var mustProp = serializedObject.FindProperty("MustBeGrounded");
                var enabled = GUI.enabled;
                GUI.enabled = mustProp.boolValue;
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "UndoIfGoingBackwards");
                GUI.enabled = enabled;

                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "FromPath", "ToPath");
            }
            else
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "MustBeGrounded",
                    "UndoIfGoingBackwards", "FromPath", "ToPath");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
