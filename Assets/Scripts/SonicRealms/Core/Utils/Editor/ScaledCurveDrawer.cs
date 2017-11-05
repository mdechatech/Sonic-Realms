using SonicRealms.Core.Internal;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [CustomPropertyDrawer(typeof(SrScaledCurve))]
    public class ScaledCurveDrawer : PropertyDrawer
    {
        private const int CurveWidth = 50;
        private const int Spacing = 5;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            var scale = prop.FindPropertyRelative("Scale");
            var curve = prop.FindPropertyRelative("Curve");
            
            EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width - CurveWidth - Spacing, pos.height), scale, label);
            
            EditorGUI.CurveField(new Rect(pos.x + pos.width - CurveWidth + Spacing, pos.y, CurveWidth - Spacing, pos.height), curve,
                Color.green, new Rect(0, 0, 1, 1), GUIContent.none);
        }
    }
}
