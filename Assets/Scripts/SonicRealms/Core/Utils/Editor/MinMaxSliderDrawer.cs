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

            var range = property.vector2Value;

            var min = range.x;
            var max = range.y;

            var attr = (MinMaxSliderAttribute) attribute;

            EditorGUI.BeginChangeCheck();

            EditorGUI.MinMaxSlider(label, new Rect(position.x, position.y, position.width - attr.LabelWidth, position.height),
                ref min, ref max, attr.Min, attr.Max);

            if (EditorGUI.EndChangeCheck())
            {
                range.x = min;
                range.y = max;
                property.vector2Value = range;
            }

            if (attr.LabelWidth > 0)
            {
                EditorGUI.LabelField(
                    new Rect(position.x + position.width - attr.LabelWidth, position.y, attr.LabelWidth, position.height),
                    string.Format(attr.LabelFormat, range.x, range.y),
                    new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });
            }
        }
    }
}