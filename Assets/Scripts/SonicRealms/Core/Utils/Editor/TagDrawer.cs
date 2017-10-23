using SonicRealms.Core.Internal;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [CustomPropertyDrawer(typeof(SrTagAttribute))]
    public class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var tooltip = RealmsEditorUtility.GetAttribute<TooltipAttribute>(property);
            if (tooltip != null)
            {
                property.stringValue = EditorGUI.TagField(position, 
                    new GUIContent(ObjectNames.NicifyVariableName(property.name), tooltip.tooltip), property.stringValue);
            }
            else
            {
                property.stringValue = EditorGUI.TagField(position, ObjectNames.NicifyVariableName(property.name),
                    property.stringValue);
            }
        }
    }
}
