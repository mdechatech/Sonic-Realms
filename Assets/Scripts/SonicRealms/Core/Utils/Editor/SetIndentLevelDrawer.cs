using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [CustomPropertyDrawer(typeof(SetIndentLevelAttribute))]
    public class SetIndentLevelDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var attr = (SetIndentLevelAttribute) attribute;
            EditorGUI.indentLevel = attr.Level;
        }

        public override float GetHeight()
        {
            return 0;
        }
    }
}
