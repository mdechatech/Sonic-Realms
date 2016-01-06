using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    public abstract class BaseReactiveEditor : UnityEditor.Editor
    {
        protected bool ShowAnimationFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowAnimationFoldout", false); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowAnimationFoldout", value); }
        }

        protected bool ShowComponentsFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowComponentsFoldout", false); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowComponentsFoldout", value); }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawAnimationFoldout();
            if(ShowAnimationFoldout)
                DrawAnimationProperties();

            DrawComponentsFoldout();
            if (ShowComponentsFoldout)
                DrawComponentsProperties();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawAnimationFoldout()
        {
            ShowAnimationFoldout = EditorGUILayout.Foldout(ShowAnimationFoldout, "Animation");
        }

        protected virtual void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "Animator");
        }

        protected virtual void DrawComponentsFoldout()
        {
            ShowComponentsFoldout = EditorGUILayout.Foldout(ShowComponentsFoldout, "Components");
        }

        protected virtual void DrawComponentsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ObjectTrigger");
        }
    }
}
