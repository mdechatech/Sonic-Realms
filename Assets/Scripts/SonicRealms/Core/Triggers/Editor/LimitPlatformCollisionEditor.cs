using UnityEditor;

namespace SonicRealms.Core.Triggers.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LimitPlatformCollision))]
    public class LimitPlatformCollisionEditor : LimiterEditorBase
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
            EditorGUILayout.HelpBox("The platform is pass-through if the player meets the conditions below.",
                MessageType.Info);

            base.OnInspectorGUI();
        }
    }
}
