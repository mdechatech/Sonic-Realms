using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(AirControl))]
    public class AirControlEditor : UnityEditor.Editor
    {
        protected bool ShowControlsFoldout
        {
            get { return EditorPrefs.GetBool("AirControlEditor.ShowControlsFoldout", false); }
            set { EditorPrefs.SetBool("AirControlEditor.ShowControlsFoldout", value); }
        }

        protected bool ShowPhysicsFoldout
        {
            get { return EditorPrefs.GetBool("AirControlEditor.ShowPhysicsFoldout", false); }
            set { EditorPrefs.SetBool("AirControlEditor.ShowPhysicsFoldout", value); }
        }

        protected bool ShowAnimationFoldout
        {
            get { return EditorPrefs.GetBool("AirControlEditor.ShowAnimationFoldout", false); }
            set { EditorPrefs.SetBool("AirControlEditor.ShowAnimationFoldout", value); }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ShowControlsFoldout = EditorGUILayout.Foldout(ShowControlsFoldout, "Controls");
            if (ShowControlsFoldout)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "InputAxis", "InvertAxis");
            }

            ShowPhysicsFoldout = EditorGUILayout.Foldout(ShowPhysicsFoldout, "Physics");
            if (ShowPhysicsFoldout)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "Acceleration", "Deceleration", "TopSpeed");
            }

            ShowAnimationFoldout = EditorGUILayout.Foldout(ShowAnimationFoldout, "Animation");
            if (ShowAnimationFoldout)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "HorizontalSpeedParameter", "VerticalSpeedParameter");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}