using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Actors.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HealthSystem), true)]
    public class HealthSystemEditor : BaseFoldoutEditor { }
}
