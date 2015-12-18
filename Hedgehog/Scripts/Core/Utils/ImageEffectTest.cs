using UnityEngine;

namespace Hedgehog.Core.Utils
{
    public class ImageEffectTest : MonoBehaviour
    {
        public Material Material;

        public void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, Material);
        }
    }
}
