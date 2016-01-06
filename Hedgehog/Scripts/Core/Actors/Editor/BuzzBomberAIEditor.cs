using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Core.Actors.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuzzBomberAI))]
    public class BuzzBomberAIEditor : BaseFoldoutEditor { }
}
