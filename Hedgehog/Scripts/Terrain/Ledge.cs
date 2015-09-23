using UnityEngine;

namespace Hedgehog.Terrain
{
    [RequireComponent(typeof(PlatformTrigger))]
    public class Ledge : MonoBehaviour
    {
        public void OnEnable()
        {
            GetComponent<PlatformTrigger>().CollisionPredicates.Add(Selector);
        }

        public void OnDisable()
        {
            GetComponent<PlatformTrigger>().CollisionPredicates.Remove(Selector);
        }

        public static bool Selector(TerrainCastHit hit)
        {
            if(hit.Source == null) 
                return (hit.Side & TerrainSide.Bottom) > 0;

            // Check must be coming from player's bottom side and be close to the top
            // of the platform
            return (hit.Side & TerrainSide.Bottom) > 0 && hit.Hit.fraction > 0.0f;
        }
    }
}
