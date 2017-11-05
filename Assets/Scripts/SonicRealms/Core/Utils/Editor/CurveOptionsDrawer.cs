using SonicRealms.Core.Internal;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [CustomPropertyDrawer(typeof(SrCurveOptionsAttribute))]
    public class CurveOptionsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType != SerializedPropertyType.AnimationCurve)
            {
                EditorGUI.LabelField(position,
                    string.Format("CurveOptions can't be used on '{0}' because it is not an AnimationCurve.",
                        ObjectNames.NicifyVariableName(property.name)),
                    new GUIStyle(EditorStyles.label) {normal = {textColor = Color.red}});

                return;
            }

            var attr = (SrCurveOptionsAttribute)attribute;

            var rect = default(Rect?);
            var color = default(Color?);

            if (attr.XMax.HasValue && attr.YMax.HasValue && attr.XMin.HasValue && attr.YMin.HasValue)
                rect = Rect.MinMaxRect(attr.XMin.Value, attr.YMin.Value, attr.XMax.Value, attr.YMax.Value);

            if (attr.Color.HasValue)
                color = FromUint(attr.Color.Value);
            
            EditorGUI.CurveField(position, property, color ?? Color.green, rect.GetValueOrDefault(), label);
        }

        private static Color32 FromUint(uint color)
        {
            return new Color32(
                (byte)((color >> 24) & 0xff),
                (byte)((color >> 16) & 0xff),
                (byte)((color >> 8) & 0xff),
                (byte)(color & 0xff));
        }
    }
}
