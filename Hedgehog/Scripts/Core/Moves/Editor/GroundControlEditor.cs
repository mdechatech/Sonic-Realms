using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(GroundControl))]
    public class GroundControlEditor : UnityEditor.Editor
    {
        protected bool ShowControlsFoldout
        {
            get { return EditorPrefs.GetBool("GroundControlEditor.ShowControlsFoldout", false); }
            set { EditorPrefs.SetBool("GroundControlEditor.ShowControlsFoldout", value); }
        }

        protected bool ShowPhysicsFoldout
        {
            get { return EditorPrefs.GetBool("GroundControlEditor.ShowPhysicsFoldout", false); }
            set { EditorPrefs.SetBool("GroundControlEditor.ShowPhysicsFoldout", value); }
        }

        protected bool ShowAnimationFoldout
        {
            get { return EditorPrefs.GetBool("GroundControlEditor.ShowAnimationFoldout", false); }
            set { EditorPrefs.SetBool("GroundControlEditor.ShowAnimationFoldout", value); }
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
                    "InputAxisFloat", "GroundedBool", "AcceleratingBool", "BrakingBool", 
                    "SpeedFloat", "AbsoluteSpeedFloat", "SurfaceAngleFloat", "ControlLockBool", 
                    "ControlLockTimerFloat", "TopSpeedBool");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}