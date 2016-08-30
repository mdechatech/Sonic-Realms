using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;

namespace SonicRealms.Level.Platforms
{
    public class GroundOnlyCollision : ReactivePlatform
    {
        public override bool IsSolid(TerrainCastHit hit)
        {
            return hit.Controller.Grounded;
        }
    }
}
