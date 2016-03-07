using SonicRealms.Core.Utils.Editor;
using UnityEditor;

namespace SonicRealms.Core.Actors.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HealthSystem), true)]
    public class HealthSystemEditor : BaseFoldoutEditor { }
}
