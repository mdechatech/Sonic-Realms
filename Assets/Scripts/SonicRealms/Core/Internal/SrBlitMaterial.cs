using UnityEngine;

namespace SonicRealms.Core.Internal
{
    [ExecuteInEditMode]
    public class SrBlitMaterial : MonoBehaviour
    {
        public Material Material;

        public void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, Material);
        }
    }
}
