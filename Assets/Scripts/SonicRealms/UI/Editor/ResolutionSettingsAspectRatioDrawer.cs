using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.UI.Editor
{
    [CustomPropertyDrawer(typeof(ResolutionSettings.AspectRatio))]
    public class ResolutionSettingsAspectRatioDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var horizontal = property.FindPropertyRelative("_horizontal");
            var vertical = property.FindPropertyRelative("_vertical");

            var labelRect = new Rect(position)
            {
                height = RealmsEditorUtility.RowHeight
            };

            var bottomRow = new Rect(position)
            {
                xMin = position.xMin + RealmsEditorUtility.IndentWidth,
                yMin = position.yMin + RealmsEditorUtility.RowHeight,
                height = RealmsEditorUtility.RowHeight
            };

            var widthRect = new Rect(bottomRow)
            {
                xMin = bottomRow.xMin,
                xMax = bottomRow.xMin + bottomRow.width * 0.5f,
                height = bottomRow.height
            };

            var heightRect = new Rect(bottomRow)
            {
                xMin = widthRect.xMax - RealmsEditorUtility.CurrentIndent + RealmsEditorUtility.IndentWidth,
                width = widthRect.width,
                height = bottomRow.height
            };

            EditorGUI.LabelField(labelRect, label);

            EditorGUIUtility.labelWidth = 50 + EditorGUI.indentLevel*RealmsEditorUtility.IndentWidth;

            EditorGUI.PropertyField(widthRect, horizontal, new GUIContent("H"));
            EditorGUI.PropertyField(heightRect, vertical, new GUIContent("V"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 30;
        }

        private void Fix(ref int a, ref int b)
        {
            a = Mathf.Max(1, a);
            b = Mathf.Max(1, b);
            /*
            var gcd = DMath.GCD(a, b);

            a /= gcd;
            b /= gcd;
            */
        }
    }
}
