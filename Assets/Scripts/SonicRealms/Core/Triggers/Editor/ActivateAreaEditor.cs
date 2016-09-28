using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Core.Triggers.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ActivateArea))]
    public class ActivateAreaEditor : LimiterEditorBase
    {
        private static readonly string[] StaticLimiterNames = new[]
        {
            "Grounded",
            "Velocity",
            "AirSpeed",
            "GroundSpeed",
            "SurfaceAngle",
            "Moves",
            "Powerups"
        };

        protected override string[] LimiterNames { get { return StaticLimiterNames; } }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("The area will activate if the player meets the conditions below.",
                MessageType.Info);

            base.OnInspectorGUI();
        }
    }
}
