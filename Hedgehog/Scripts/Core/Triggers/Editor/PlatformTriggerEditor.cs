using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Triggers.Editor
{
    [CustomEditor(typeof(PlatformTrigger)), CanEditMultipleObjects]
    public class PlatformTriggerEditor : BaseTriggerEditor
    {
        protected static bool ShowPlatformEvents
        {
            get { return EditorPrefs.GetBool("PlatformTriggerEditor.ShowPlatformEvents", false); }
            set { EditorPrefs.SetBool("PlatformTriggerEditor.ShowPlatformEvents", value); }
        }

        protected static bool ShowSurfaceEvents
        {
            get { return EditorPrefs.GetBool("PlatformTriggerEditor.ShowSurfaceEvents", false); }
            set { EditorPrefs.SetBool("PlatformTriggerEditor.ShowSurfaceEvents", value); }
        }

        protected static bool ShowSound
        {
            get { return EditorPrefs.GetBool("PlatformTriggerEditor.ShowSound", false); }
            set { EditorPrefs.SetBool("PlatformTriggerEditor.ShowSound", value); }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ShowPlatformEvents = EditorGUILayout.Foldout(ShowPlatformEvents, "Platform Events");
            if (ShowPlatformEvents)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "OnPlatformEnter", "OnPlatformStay", "OnPlatformExit");
            }

            ShowSurfaceEvents = EditorGUILayout.Foldout(ShowSurfaceEvents, "Surface Events");
            if (ShowSurfaceEvents)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "OnSurfaceEnter", "OnSurfaceStay", "OnSurfaceExit");
            }

            ShowSound = EditorGUILayout.Foldout(ShowSound, "Sound");
            if (ShowSound)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                    "PlatformEnterSound", "PlatformLoopSound", "PlatformExitSound",
                    "SurfaceEnterSound", "SurfaceLoopSound", "SurfaceExitSound");
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
