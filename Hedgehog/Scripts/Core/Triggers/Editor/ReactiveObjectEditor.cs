using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(ReactiveObject), true)]
    [CanEditMultipleObjects]
    public class ReactiveObjectEditor : BaseReactiveEditor
    {
        public override void OnInspectorGUI()
        {
            if (targets.Length == 1)
            {
                var instance = target as ReactiveObject;
                if (instance.GetComponent<ReactiveArea>() == null && 
                    instance.GetComponent<ReactivePlatform>() == null)
                {
                    EditorGUILayout.HelpBox("This effect has no activator. Try adding an " +
                                            "\"Activate Platform\" or \"Activate Area\" component to this object!",
                        MessageType.Info);
                }
            }

            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "Animator", "ObjectTrigger",
                HedgehogEditorGUIUtility.ScriptPropertyName);

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }

        protected override void DrawAnimationProperties()
        {
            // nothing here... YET
        }
    }
}
