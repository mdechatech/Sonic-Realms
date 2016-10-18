using SonicRealms.Core.Utils;
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

            var newHorizontal = horizontal.intValue;
            var newVertical = vertical.intValue;

            float rectX = position.x, rectY = position.y;

            var labelRect = new Rect(rectX, rectY, position.width, position.height*0.5f);
            rectY += labelRect.height;
            
            var horizontalRect = new Rect(
                rectX,
                rectY, 
                position.width*0.4f, 
                position.height*0.5f);

            rectX += horizontalRect.width;

            var colonRect = new Rect(
                rectX,
                rectY,
                50,
                position.height * 0.5f);

            rectX += colonRect.width;

            var verticalRect = new Rect(
                rectX,
                rectY,
                position.width*0.4f,
                position.height * 0.5f);

            Fix(ref newHorizontal, ref newVertical);

            EditorGUI.LabelField(labelRect, label);

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 25;

            var indentLevel = EditorGUI.indentLevel;
            --EditorGUI.indentLevel;
            
            newHorizontal = EditorGUI.IntField(horizontalRect, "H", newHorizontal);

            EditorGUI.LabelField(colonRect, ":");

            newVertical = EditorGUI.IntField(verticalRect, "V", newVertical);

            EditorGUIUtility.labelWidth = labelWidth;

            horizontal.intValue = newHorizontal;
            vertical.intValue = newVertical;

            EditorGUI.indentLevel = indentLevel;
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
