using Hedgehog.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace Hedgehog.Core.Moves.Editor
{
    [CustomEditor(typeof(AirSource))]
    public class AirSourceEditor : MoveEditor
    {
        protected override void DrawControlProperties()
        {
            base.DrawControlProperties();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "LimitedDuration");
            if (!serializedObject.FindProperty("LimitedDuration").boolValue) return;

            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "Duration");
            var enabled = GUI.enabled;
            GUI.enabled = Application.isPlaying;
            HedgehogEditorGUIUtility.DrawProperties(serializedObject, "RemainingTime");
            GUI.enabled = enabled;
        }
    }
}
