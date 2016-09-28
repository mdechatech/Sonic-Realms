using System.Linq;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Triggers.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ActivatePlatform))]
    public class ActivatePlatformEditor : LimiterEditorBase
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
            serializedObject.Update();

            EditorGUILayout.HelpBox("The platform will activate if the player meets the conditions below.",
                MessageType.Info);

            RealmsEditorUtility.DrawProperties(serializedObject,
                "WhenPreColliding",
                "WhenColliding",
                "WhenOnSurface");

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();
        }
    }
}
