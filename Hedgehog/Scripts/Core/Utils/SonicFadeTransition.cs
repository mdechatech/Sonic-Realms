using UnityEngine;

namespace Hedgehog.Core.Utils
{
    public class SonicFadeTransition : MonoBehaviour
    {
        public Material Material;

        [Range(0.0f, 1.0f)]
        public float Progress;

        public void Reset()
        {
            Progress = 0.0f;
        }

        public void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Material.SetFloat("_Blue", Mathf.Clamp01(Progress*3.0f));
            Material.SetFloat("_Green", Mathf.Clamp01(Progress*3.0f - 1.0f));
            Material.SetFloat("_Red", Mathf.Clamp01(Progress*3.0f - 2.0f));
            Graphics.Blit(src, dest, Material);
        }
    }
}
