using Hedgehog.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(Move))]
    public class MoveEditor : UnityEditor.Editor
    {
        protected bool ShowControlFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowControlFoldout", false); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowControlFoldout", value); }
        }

        protected bool ShowPhysicsFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowPhysicsFoldout", false); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowPhysicsFoldout", value); }
        }

        protected bool ShowEventsFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowEventsFoldout", false); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowEventsFoldout", value); }
        }

        protected bool ShowAnimationFoldout
        {
            get { return EditorPrefs.GetBool(GetType().Name + ".ShowAnimationFoldout", false); }
            set { EditorPrefs.SetBool(GetType().Name + ".ShowAnimationFoldout", value); }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawAnimationFoldout();
            if (ShowAnimationFoldout)
                DrawAnimationProperties();

            DrawControlFoldout();
            if(ShowControlFoldout)
                DrawControlProperties();

            DrawEventsFoldout();
            if (ShowEventsFoldout)
                DrawEventsProperties();

            DrawPhysicsFoldout();
            if(ShowPhysicsFoldout)
                DrawPhysicsProperties();

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

        protected virtual void DrawEventsFoldout()
        {
            ShowEventsFoldout = EditorGUILayout.Foldout(ShowEventsFoldout, "Events");
        }

        protected virtual void DrawEventsProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "OnActive", "OnEnd", "OnAvailable", "OnUnavailable");
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
