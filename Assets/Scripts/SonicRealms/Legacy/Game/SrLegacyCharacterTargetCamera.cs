using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Legacy.Game
{
    [RequireComponent(typeof(CameraController))]
    public class SrLegacyCharacterTargetCamera : MonoBehaviour
    {
        public string CharacterName;

        public void Start()
        {
            var target = SrLegacyGameManager.Instance.GetCharacter(CharacterName).transform;
            if (!target) return;

            GetComponent<CameraController>().Target = target;
        }
    }
}
