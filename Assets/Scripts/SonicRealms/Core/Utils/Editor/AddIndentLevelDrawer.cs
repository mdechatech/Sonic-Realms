using SonicRealms.Core.Internal;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    [CustomPropertyDrawer(typeof(SrAddIndentLevelAttribute))]
    public class AddIndentLevelDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            var attr = (SrAddIndentLevelAttribute) attribute;
            EditorGUI.indentLevel += attr.Amount;
        }

        public override float GetHeight()
        {
            return 0;
        }
    }
}
