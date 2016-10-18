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

            var widthRect = new Rect(position) {xMax = position.x + position.width*0.5f};
            var heightRect = new Rect(position) {xMin = widthRect.xMax, xMax = widthRect.xMax + widthRect.width};
            
            var widthLabel = EditorGUI.BeginProperty(widthRect, new GUIContent("Width"), widthProp);

            //var widthContentRect = EditorGUI.PrefixLabel(widthRect, widthLabel);

            --EditorGUI.indentLevel;

            EditorGUI.PropertyField(widthRect, widthProp, widthLabel);

            EditorGUI.EndProperty();
            
            var heightLabel = EditorGUI.BeginProperty(heightRect, new GUIContent("Height"), heightProp);

            //var heightContentRect = EditorGUI.PrefixLabel(heightRect, heightLabel);

            --EditorGUI.indentLevel;

            EditorGUI.PropertyField(heightRect, heightProp, heightLabel);

            EditorGUI.EndProperty();
        }
    }
}
