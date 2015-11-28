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

        public override void OnInspectorGUI()
        {
            DrawAnimationFoldout();
            if(ShowAnimationFoldout)
                DrawAnimationProperties();
        }

        protected virtual void DrawAnimationFoldout()
        {
            ShowAnimationFoldout = EditorGUILayout.Foldout(ShowAnimationFoldout, "Animation");
        }

        protected abstract void DrawAnimationProperties();
    }
}
