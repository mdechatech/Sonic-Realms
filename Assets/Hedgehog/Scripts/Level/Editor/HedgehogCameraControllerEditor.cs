using Hedgehog.Core.Utils.Editor;
using UnityEditor;

namespace Hedgehog.Level.Editor
{
    [CustomEditor(typeof(CameraController))]
    [CanEditMultipleObjects]
    public class HedgehogCameraControllerEditor : BaseFoldoutEditor
    {
    }
}
