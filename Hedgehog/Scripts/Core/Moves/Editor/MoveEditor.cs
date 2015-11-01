using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(Move), true)]
    [CanEditMultipleObjects]
    public class MoveEditor : UnityEditor.Editor
    {
        protected bool ShowControlFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowControlFoldout", false); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowControlFoldout", value); }
        }

        protected bool ShowPhysicsFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowPhysicsFoldout"); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowPhysicsFoldout", value); }
        }

        protected bool ShowAnimationFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowAnimationFoldout"); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowAnimationFoldout", value); }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawControlFoldout();
            if(ShowControlFoldout)
                DrawControlProperties();

            DrawPhysicsFoldout();
            if(ShowPhysicsFoldout)
                DrawPhysicsProperties();

            DrawAnimationFoldout();
            if(ShowAnimationFoldout)
                DrawAnimationProperties();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawControlFoldout()
        {
            ShowControlFoldout = EditorGUILayout.Foldout(ShowControlFoldout, "Control");
        }

        protected virtual void DrawControlProperties()
        {

        }

        protected virtual void DrawPhysicsFoldout()
        {
            ShowPhysicsFoldout = EditorGUILayout.Foldout(ShowPhysicsFoldout, "Physics");
        }

        protected virtual void DrawPhysicsProperties()
        {

        }

        protected virtual void DrawAnimationFoldout()
        {
            ShowAnimationFoldout = EditorGUILayout.Foldout(ShowAnimationFoldout, "Animation");
        }


        protected virtual void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "ActiveTrigger", "ActiveBool", "AvailableBool");
        }
    }
}
