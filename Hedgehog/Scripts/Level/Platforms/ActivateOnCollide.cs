using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Generic platform that activates the object trigger when a controller collides with it.
    /// </summary>
    public class ActivateOnCollide : ReactivePlatform
    {
        public override void OnPlatformEnter(TerrainCastHit hit)
        {
            ActivateObject(hit.Controller);
        }

        public override void OnPlatformExit(TerrainCastHit hit)
        {
            DeactivateObject(hit.Controller);
        }
    }
}
