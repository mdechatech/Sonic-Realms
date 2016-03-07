using UnityEngine;

namespace SonicRealms.Core.Utils
{
    [ExecuteInEditMode]
    public class BlitMaterial : MonoBehaviour
    {
        public Material Material;

        public void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, Material);
        }
    }
}
