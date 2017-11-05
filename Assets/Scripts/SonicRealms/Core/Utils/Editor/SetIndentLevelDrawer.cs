using SonicRealms.Core.Internal;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [CustomPropertyDrawer(typeof(SrSetIndentLevelAttribute))]
    public class SetIndentLevelDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var attr = (SrSetIndentLevelAttribute) attribute;
            EditorGUI.indentLevel = attr.Level;
        }

        public override float GetHeight()
        {
            return 0;
        }
    }
}
