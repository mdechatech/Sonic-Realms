using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Actors.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MotobugAI))]
    public class MotobugAIEditor : BaseFoldoutEditor { }
}
