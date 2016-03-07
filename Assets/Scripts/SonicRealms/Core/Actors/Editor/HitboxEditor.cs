using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Core.Actors.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Hitbox), true)]
    public class HitboxEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FoldoutDrawer.DoFoldoutsLayout(serializedObject);
        }
    }
}
