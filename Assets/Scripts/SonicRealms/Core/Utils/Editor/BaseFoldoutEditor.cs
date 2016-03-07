using UnityEditor;

namespace SonicRealms.Core.Utils.Editor
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
