using SonicRealms.Core.Internal;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Gives friction to a platform, allowing slippery surfaces.
    /// </summary>
    public class Ice : ReactivePlatform
    {
        /// <summary>
        /// The friction coefficient; smaller than one and the surface becomes slippery.
        /// </summary>
        [SerializeField]
        [Tooltip("The friction coefficient; smaller than one and the surface becomes slippery.")]
        public float Friction;

        public override void Reset()
        {
            base.Reset();
            Friction = 0.2f;
        }

        public override void Awake()
        {
            base.Awake();
            if (SrMath.Equalsf(Friction)) Friction += SrMath.Epsilon;
        }

        // Applies new physics values based on friction.
        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            var groundControl = collision.Controller.GetComponent<MoveManager>().Get<GroundControl>();
            groundControl.Acceleration *= Friction;
            groundControl.Deceleration *= Friction;
            collision.Controller.GroundFriction *= Friction;
        }

        // Restores old physics values.
        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            var groundControl = collision.Controller.GetComponent<MoveManager>().Get<GroundControl>();
            groundControl.Acceleration /= Friction;
            groundControl.Deceleration /= Friction;
            collision.Controller.GroundFriction /= Friction;
        }
    }
}
