using Hedgehog.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ReactiveObject), true)]
    [CanEditMultipleObjects]
    public class ReactiveObjectEditor : BaseReactiveEditor
    {
        public override void OnInspectorGUI()
        {
            DrawActivatorCheck(targets);

            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "Animator", "ObjectTrigger",
                HedgehogEditorGUIUtility.ScriptPropertyName);

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }

        public static void DrawActivatorCheck(Object[] targets)
        {
            if (targets.Length != 1) return;

            var instance = targets[0] as ReactiveObject;
            if (instance.GetComponent<ReactiveArea>() == null &&
                instance.GetComponent<ReactivePlatform>() == null)
            {
                EditorGUILayout.HelpBox("This effect has no activator. Try adding an " +
                                        "\"Activate Platform\" or \"Activate Area\" component to this object!",
                    MessageType.Info);
            }
        }

        protected override void DrawAnimationProperties()
        {
            // nothing here... YET
        }
    }
}
