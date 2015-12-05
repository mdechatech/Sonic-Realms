using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Generic platform that activates the object trigger when a controller stands on it.
    /// </summary>
    public class ActivateOnSurface : ReactivePlatform
    {
        public override void OnSurfaceEnter(TerrainCastHit hit)
        {
            ActivateObject(hit.Controller);
        }

        public override void OnSurfaceExit(TerrainCastHit hit)
        {
            DeactivateObject(hit.Controller);
        }
    }

}
