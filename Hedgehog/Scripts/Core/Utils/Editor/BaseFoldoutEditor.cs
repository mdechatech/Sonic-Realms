using UnityEditor;

namespace Hedgehog.Core.Utils.Editor
{
    [CanEditMultipleObjects]
    public class BaseFoldoutEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FoldoutDrawer.DoFoldoutsLayout(serializedObject);
        }
    }
}
