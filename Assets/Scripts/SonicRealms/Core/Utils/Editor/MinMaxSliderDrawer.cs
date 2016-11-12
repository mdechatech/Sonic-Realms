using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [CustomPropertyDrawer(typeof (MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2)
                return;

            var attr = (MinMaxSliderAttribute) attribute;

            const float minMaxLabelWidth = 30;
            var minMaxWidth = EditorGUIUtility.fieldWidth + minMaxLabelWidth;

            var labelRect = new Rect(position)
            {
                width = EditorGUIUtility.labelWidth
            };

            var contentRect = new Rect(position)
            {
                xMin = labelRect.xMax,
            };

            var minRect = new Rect(contentRect)
            {
                height = RealmsEditorUtility.RowHeight,
                width = minMaxWidth
            };  

            var maxRect = new Rect(contentRect)
            {
                height = RealmsEditorUtility.RowHeight,
                xMin = Mathf.Max(minRect.xMax, position.xMax - minMaxWidth),
                width = minMaxWidth
            };

            var sliderRect = new Rect(contentRect)
            {
                yMin = position.yMin + RealmsEditorUtility.RowHeight,
                height = RealmsEditorUtility.RowHeight
            };

            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();

            var range = property.vector2Value;

            var min = range.x;
            var max = range.y;

            EditorGUI.LabelField(labelRect, label);

            EditorGUIUtility.labelWidth = minMaxLabelWidth;

            min = Mathf.Clamp(EditorGUI.FloatField(minRect, "Min", min), attr.Min, max);
            max = Mathf.Clamp(EditorGUI.FloatField(maxRect, "Max", max), min, attr.Max);

            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, attr.Min, attr.Max);

            if (EditorGUI.EndChangeCheck())
            {
                range.x = min;
                range.y = max;
                property.vector2Value = range;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return RealmsEditorUtility.RowHeight*2;
        }
    }
}