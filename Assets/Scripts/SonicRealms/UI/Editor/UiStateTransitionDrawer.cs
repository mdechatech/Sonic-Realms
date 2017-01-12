using UnityEditor;
using UnityEngine;

namespace SonicRealms.UI.Editor
{
    [CustomPropertyDrawer(typeof(UiStateTransition))]
    public class UiStateTransitionDrawer : PropertyDrawer
    {
        public const float PropertyHeight = 48;

        private bool _hasInitialized;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float leftWidth = 100;
            const float positionPad = 4;

            const float labelWidth = 35;

            const float toggleLabelWidth = 35;
            const float toggleWidth = 25 + toggleLabelWidth;

            const float handlerLabelWidth = 64;
            // const float handlerSpacing = 8;

            var fixedWidth = labelWidth + toggleLabelWidth + handlerLabelWidth + 32;

            var handlerFieldWidth = Mathf.Clamp((position.width - fixedWidth) * 0.5f, 32, 300);

            fixedWidth += handlerFieldWidth;

            var fieldWidth = Mathf.Clamp(position.width - fixedWidth, 16, 99999);

            position = new Rect(position)
            {
                yMin = position.yMin + positionPad,
                yMax = position.yMax - positionPad
            };

            const float fromToPad = 2;

            var fromRect = new Rect(position)
            {
                xMax = position.xMin + leftWidth,
                yMin = position.yMin + fromToPad,
                yMax = position.yMin + position.height * 0.5f - fromToPad
            };

            var toRect = new Rect(position) {

                xMax = position.xMin + leftWidth,
                yMin = position.yMax - position.height * 0.5f + fromToPad,
                yMax = position.yMax - fromToPad,
            };

            var fromLabelRect = new Rect(fromRect)
            {
                xMax = fromRect.xMin + labelWidth
            };

            var toLabelRect = new Rect(toRect)
            {
                xMax = toRect.xMin + labelWidth
            };

            // From label and To label
            EditorGUI.LabelField(fromLabelRect, "From");
            EditorGUI.LabelField(toLabelRect, "To");


            // From Any State toggle and To Any State toggle

            var fromAnyRect = ShiftRight(fromLabelRect, toggleWidth);
            var toAnyRect = ShiftRight(toLabelRect, toggleWidth);

            var fromAnyStateProp = property.FindPropertyRelative("_fromAnyState");
            var toAnyStateProp = property.FindPropertyRelative("_toAnyState");

            EditorGUIUtility.labelWidth = toggleLabelWidth;

            EditorGUI.PropertyField(fromAnyRect, fromAnyStateProp, new GUIContent(" Any"));
            EditorGUI.PropertyField(toAnyRect, toAnyStateProp, new GUIContent(" Any"));

            // From/To fields
            var fromFieldRect = ShiftRight(fromAnyRect, fieldWidth);
            var toFieldRect = ShiftRight(toAnyRect, fieldWidth);

            if (!fromAnyStateProp.boolValue)
                EditorGUI.PropertyField(fromFieldRect, property.FindPropertyRelative("_fromState"), GUIContent.none);

            if (!toAnyStateProp.boolValue)
                EditorGUI.PropertyField(toFieldRect, property.FindPropertyRelative("_toState"), GUIContent.none);

            
            // Handler field
            var handlerRect = new Rect
            {
                xMin = fromFieldRect.xMax,
                xMax = fromFieldRect.xMax + handlerFieldWidth + handlerLabelWidth,
                y = fromFieldRect.center.y,
                height = 16
            };

            EditorGUIUtility.labelWidth = handlerLabelWidth;

            EditorGUI.PropertyField(handlerRect, property.FindPropertyRelative("_handler"), new GUIContent("  Handler"));
        }

        private Rect ShiftRight(Rect rect, float width)
        {
            return new Rect(rect)
            {
                xMin = rect.xMax,
                xMax = rect.xMax + width
            };
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyHeight;
        }
    }
}
