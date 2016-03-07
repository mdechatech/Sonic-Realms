using UnityEngine;

namespace SonicRealms.Level
{
    [RequireComponent(typeof(CameraController))]
    public class CharacterTargetCamera : MonoBehaviour
    {
        public string CharacterName;

        public void Start()
        {
            var target = GameManager.Instance.GetCharacter(CharacterName).transform;
            if (!target) return;

            GetComponent<CameraController>().Target = target;
        }
    }
}
