using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [CustomPropertyDrawer(typeof(EnumSelectionGridAttribute))]
    public class EnumSelectionGridDrawer : PropertyDrawer
    {
        private const float ButtonSpacing = 3;

        private string[] _options;
        private EnumSelectionGridAttribute _attribute;

        private void Initialize()
        {
            _attribute = (EnumSelectionGridAttribute)attribute;

            if (typeof(Enum).IsAssignableFrom(fieldInfo.FieldType))
            {
                if(_attribute.NicifyNames)
                {
                    _options = Enum.GetNames(fieldInfo.FieldType).Select<string, string>
                        (ObjectNames.NicifyVariableName).ToArray();
                }
                else
                {
                    _options = Enum.GetNames(fieldInfo.FieldType);
                }
            }
            else
            {
                _options = new string[0];
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                EditorGUI.LabelField(position,

                    string.Format("EnumSelectionGrid can't be used on '{0}' because it is not an Enum.",
                        property.displayName),

                    new GUIStyle(EditorStyles.label)
                    {
                        normal = new GUIStyleState {textColor = Color.red},
                        alignment = TextAnchor.MiddleCenter
                    });

                return;
            }

            if (_options == null)
                Initialize();

            var labelRect = new Rect(position) {xMax = position.xMin + EditorGUIUtility.labelWidth};
            var propertyRect = new Rect(labelRect) {xMin = labelRect.xMax, xMax = position.xMax};

            GUIStyle style;

            var buttonWidth = Mathf.CeilToInt(
                propertyRect.width - (ButtonSpacing*Mathf.Max(0, _options.Length - 1)))/_options.Length;

            if (buttonWidth < _attribute.MinElementWidth)
            {
                style = new GUIStyle(GUI.skin.button) {fixedWidth = _attribute.MinElementWidth};
            }
            else
            {
                style = GUI.skin.button;
            }

            EditorGUI.LabelField(labelRect, label);
            property.enumValueIndex = GUI.SelectionGrid(propertyRect, property.enumValueIndex, _options, _options.Length, style);
           
        }

    }
}
