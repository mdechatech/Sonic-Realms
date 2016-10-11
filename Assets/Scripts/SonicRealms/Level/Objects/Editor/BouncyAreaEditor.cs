using SonicRealms.Core.Utils;
using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Level.Objects.Editor
{
    [CustomEditor(typeof (BouncyArea))]
    public class BouncyAreaEditor : BaseRealmsEditor
    {
        private SerializedProperty _remapBounceAnglesProp;
        private SerializedProperty _mapBounceAngleToVelocityProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            _remapBounceAnglesProp = serializedObject.FindProperty("_remapBounceAngles");
            _mapBounceAngleToVelocityProp = serializedObject.FindProperty("_mapBounceAngleToVelocity");
        }

        protected override void DrawProperty(PropertyData property, GUIContent label)
        {
            // Draw curves only if their checkboxes are ticked
            if (property.Property.name == "_remapBounceAnglesCurve")
            {
                if (_remapBounceAnglesProp.boolValue)
                {
                    base.DrawProperty(property, new GUIContent("Curve", label.tooltip));
                }
            }
            else if (property.Property.name == "_bounceAngleToVelocityCurve")
            {
                if (_mapBounceAngleToVelocityProp.boolValue)
                {
                    base.DrawProperty(property, new GUIContent("Curve", label.tooltip));
                }
            }
            else
            {
                base.DrawProperty(property, label);
            }
        }
    }
}
