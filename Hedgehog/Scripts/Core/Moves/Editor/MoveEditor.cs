using System.Reflection;
using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(Move), true)]
    public class MoveEditor : UnityEditor.Editor
    {
        protected bool ShowControlFoldout
        {
            get { return EditorPrefs.GetBool(target.GetType().Name + ".ShowControlFoldout", false); }
            set { EditorPrefs.SetBool(target.GetType().Name + ".ShowControlFoldout", value); }
        }

        protected bool ShowPhysicsFoldout
        {
            get { return EditorPrefs.GetBool(target.GetType().Name + ".ShowPhysicsFoldout", false); }
            set { EditorPrefs.SetBool(target.GetType().Name + ".ShowPhysicsFoldout", value); }
        }

        protected bool ShowEventsFoldout
        {
            get { return EditorPrefs.GetBool(target.GetType().Name + ".ShowEventsFoldout", false); }
            set { EditorPrefs.SetBool(target.GetType().Name + ".ShowEventsFoldout", value); }
        }

        protected bool ShowAnimationFoldout
        {
            get { return EditorPrefs.GetBool(target.GetType().Name + ".ShowAnimationFoldout", false); }
            set { EditorPrefs.SetBool(target.GetType().Name + ".ShowAnimationFoldout", value); }
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
            // If this method is overridden then back out so we don't draw duplicate properties
            if (GetType()
                .GetMethod("DrawControlProperties", BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof (MoveEditor))
                return;

            // By default draw every property that isn't inherited from Move
            HedgehogEditorGUIUtility.DrawExcluding(serializedObject,
                "OnActive", "OnEnd", "OnAvailable", "OnUnavailable", "OnAdd", "OnRemove",
                "Animator", "CurrentState", "InputActivated", "InputEnabled",
                "ActiveTrigger", "ActiveBool", "AvailableBool",
                HedgehogEditorGUIUtility.ScriptPropertyName);
        }

        protected virtual void DrawPhysicsFoldout()
        {
            // If DrawPhysicsProperties isn't overriden then we have nothing to draw inside the foldout
            if (GetType()
                .GetMethod("DrawPhysicsProperties", BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType == typeof(MoveEditor))
                return;

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
                "OnActive", "OnEnd", "OnAvailable", "OnUnavailable", "OnAdd", "OnRemove");
        }

        protected virtual void DrawAnimationFoldout()
        {
            ShowAnimationFoldout = EditorGUILayout.Foldout(ShowAnimationFoldout, "Animation");
        }

        protected virtual void DrawAnimationProperties()
        {
            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "Animator", "ActiveTrigger", "ActiveBool", "AvailableBool");
        }
    }
}
