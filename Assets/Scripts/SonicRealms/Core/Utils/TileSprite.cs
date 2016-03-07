using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class TileSprite : MonoBehaviour
    {
        public float TotalWidth;
        public float TotalHeight;

        public Renderer[] Sprites;

        public void Reset()
        {
            Sprites = GetComponentsInChildren<SpriteRenderer>();
        }

        public void Start()
        {
            foreach (var renderer in Sprites)
            {
                var times = TotalWidth/(int) renderer.bounds.size.x;
                var pos = renderer.bounds.max.x;

                for (var i = 0; i < times; ++i)
                {
                    var tile = Instantiate(renderer);
                    tile.transform.SetParent(renderer.transform.parent);
                    tile.transform.position = new Vector3(pos, renderer.transform.position.y);
                    pos += renderer.bounds.size.x;
                }
            }
        }
    }
}
