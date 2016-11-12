using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.UI
{
    [CustomPropertyDrawer(typeof(ResolutionSettings.ScreenSize))]
    public class ResolutionSettingsScreenSizeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var widthProp = property.FindPropertyRelative("_width");
            var heightProp = property.FindPropertyRelative("_height");

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
                xMax = bottomRow.xMin + bottomRow.width*0.5f,
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

            EditorGUI.PropertyField(widthRect, widthProp, new GUIContent("Width"));
            EditorGUI.PropertyField(heightRect, heightProp, new GUIContent("Height"));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return RealmsEditorUtility.RowHeight*2;
        }
    }
}
