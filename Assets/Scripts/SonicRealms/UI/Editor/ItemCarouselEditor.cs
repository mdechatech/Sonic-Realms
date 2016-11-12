using SonicRealms.Core.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.UI.Editor
{
    [CustomEditor(typeof(ItemCarousel))]
    public class ItemCarouselEditor : BaseRealmsEditor
    {
        private SerializedProperty _setItemsToChildrenProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            _setItemsToChildrenProperty = serializedObject.FindProperty("_setItemsToChildren");
        }

        protected override void DrawProperty(PropertyData property, GUIContent label)
        {
            if (property.Name == "_items")
            {
                GUI.enabled = !_setItemsToChildrenProperty.boolValue;
                base.DrawProperty(property, label);
                GUI.enabled = true;

                return;
            }

            base.DrawProperty(property, label);
        }
    }
}
