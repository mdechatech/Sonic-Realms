using System.Collections.Generic;
using System.Reflection;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Moves.Editor
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

        protected bool ShowSoundFoldout
        {
            get { return EditorPrefs.GetBool(target.GetType().Name + ".ShowSoundFoldout", false); }
            set { EditorPrefs.SetBool(target.GetType().Name + ".ShowSoundFoldout", value); }
        }

        protected bool ShowDebugFoldout
        {
            get { return EditorPrefs.GetBool(target.GetType().Name + ".ShowDebugFoldout", false); }
            set { EditorPrefs.SetBool(target.GetType().Name + ".ShowDebugFoldout", value); }
        }

        protected List<SerializedProperty> ControlFoldoutProperties;
        protected List<SerializedProperty> PhysicsFoldoutProperties;
        protected List<SerializedProperty> EventsFoldoutProperties;
        protected List<SerializedProperty> AnimationFoldoutProperties;
        protected List<SerializedProperty> SoundFoldoutProperties;
        protected List<SerializedProperty> DebugFoldoutProperties; 

        public void OnEnable()
        {
            GetFoldoutProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawControlFoldout();
            if (ShowControlFoldout) DrawControlProperties();

            DrawPhysicsFoldout();
            if (ShowPhysicsFoldout) DrawPhysicsProperties();

            DrawEventsFoldout();
            if (ShowEventsFoldout) DrawEventsProperties();

            DrawAnimationFoldout();
            if (ShowAnimationFoldout) DrawAnimationProperties();

            DrawSoundFoldout();
            if(ShowSoundFoldout) DrawSoundProperties();

            var enabled = GUI.enabled;
            GUI.enabled = Application.isPlaying;
            DrawDebugFoldout();
            if (ShowDebugFoldout) DrawDebugProperties();
            GUI.enabled = enabled;

            serializedObject.ApplyModifiedProperties();
        }

        public void GetFoldoutProperties()
        {
            ControlFoldoutProperties = new List<SerializedProperty>();
            PhysicsFoldoutProperties = new List<SerializedProperty>();
            EventsFoldoutProperties = new List<SerializedProperty>();
            AnimationFoldoutProperties = new List<SerializedProperty>();
            SoundFoldoutProperties = new List<SerializedProperty>();
            DebugFoldoutProperties = new List<SerializedProperty>();

            var type = serializedObject.targetObject.GetType();
            var iterator = serializedObject.GetIterator();
            while(iterator.NextVisible(true))
            {
                var field = type.GetField(iterator.name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field == null || field.Name == HedgehogEditorGUIUtility.ScriptPropertyName) continue;

                var found = false;
                foreach (var attribute in field.GetCustomAttributes(true))
                {
                    if (attribute is ControlFoldoutAttribute)
                    {
                        ControlFoldoutProperties.Add(iterator.Copy());
                        found = true;
                        break;
                    }

                    if (attribute is PhysicsFoldoutAttribute)
                    {
                        PhysicsFoldoutProperties.Add(iterator.Copy());
                        found = true;
                        break;
                    }

                    if (attribute is EventsFoldoutAttribute)
                    {
                        EventsFoldoutProperties.Add(iterator.Copy());
                        found = true;
                        break;
                    }

                    if (attribute is AnimationFoldoutAttribute)
                    {
                        AnimationFoldoutProperties.Add(iterator.Copy());
                        found = true;
                        break;
                    }

                    if (attribute is SoundFoldoutAttribute)
                    {
                        SoundFoldoutProperties.Add(iterator.Copy());
                        found = true;
                        break;
                    }

                    if (attribute is DebugFoldoutAttribute)
                    {
                        DebugFoldoutProperties.Add(iterator.Copy());
                        found = true;
                        break;
                    }
                }

                if (found) continue;
                ControlFoldoutProperties.Add(iterator.Copy());
            }
        }

        protected virtual void DrawControlFoldout()
        {
            ShowControlFoldout = EditorGUILayout.Foldout(ShowControlFoldout, "Control");
        }

        protected virtual void DrawControlProperties()
        {
            foreach (var property in ControlFoldoutProperties) EditorGUILayout.PropertyField(property);
        }

        protected virtual void DrawPhysicsFoldout()
        {
            if (PhysicsFoldoutProperties.Count == 0) return;
            ShowPhysicsFoldout = EditorGUILayout.Foldout(ShowPhysicsFoldout, "Physics");
        }

        protected virtual void DrawPhysicsProperties()
        {
            foreach (var property in PhysicsFoldoutProperties) EditorGUILayout.PropertyField(property);
        }

        protected virtual void DrawEventsFoldout()
        {
            if (EventsFoldoutProperties.Count == 0) return;
            ShowEventsFoldout = EditorGUILayout.Foldout(ShowEventsFoldout, "Events");
        }

        protected virtual void DrawEventsProperties()
        {
            foreach (var property in EventsFoldoutProperties) EditorGUILayout.PropertyField(property);
        }

        protected virtual void DrawAnimationFoldout()
        {
            if (AnimationFoldoutProperties.Count == 0) return;
            ShowAnimationFoldout = EditorGUILayout.Foldout(ShowAnimationFoldout, "Animation");
        }

        protected virtual void DrawAnimationProperties()
        {
            foreach (var property in AnimationFoldoutProperties) EditorGUILayout.PropertyField(property);
        }

        protected virtual void DrawSoundFoldout()
        {
            if (SoundFoldoutProperties.Count == 0) return;
            ShowSoundFoldout = EditorGUILayout.Foldout(ShowSoundFoldout, "Sound");
        }

        protected virtual void DrawSoundProperties()
        {
            foreach (var property in SoundFoldoutProperties) EditorGUILayout.PropertyField(property);
        }

        protected virtual void DrawDebugFoldout()
        {
            if (DebugFoldoutProperties.Count == 0) return;
            ShowDebugFoldout = EditorGUILayout.Foldout(ShowDebugFoldout, "Debug");
        }

        protected virtual void DrawDebugProperties()
        {
            foreach (var property in DebugFoldoutProperties) EditorGUILayout.PropertyField(property);
        }
    }
}
