using Hedgehog.Core.Actors;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// The special move performed when Sonic has the bubble shield. Dives down and then bounces back up.
    /// </summary>
    public class BubbleSpecial : DoubleJump
    {
        /// <summary>
        /// The velocity at which to dive, in units per second.
        /// </summary>
        [Tooltip("The velocity at which to dive, in units per second.")]
        public Vector2 DiveVelocity;

        /// <summary>
        /// The velocity at which to bounce, in units per second.
        /// </summary>
        [Tooltip("The velocity at which to bounce, in units per second.")]
        public Vector2 BounceVelocity;

        /// <summary>
        /// Whether to give the controller an air source while underwater.
        /// </summary>
        [Tooltip("Whether to give the controller an air source while underwater.")]
        public bool GiveAir;
        protected BreathMeter BreathMeter;

        public override void Reset()
        {
            base.Reset();
            DiveVelocity = new Vector2(0.0f, -4.8f);
            BounceVelocity = new Vector2(0.0f, 3.3f);
            GiveAir = true;
        }

        public override void OnManagerAdd()
        {
            base.OnManagerAdd();
            BreathMeter = Controller.GetComponent<BreathMeter>();
            if (BreathMeter != null && GiveAir) BreathMeter.HasAir = true;
        }

        public override void OnManagerRemove()
        {
            base.OnManagerRemove();
            if (BreathMeter != null && GiveAir) BreathMeter.HasAir = false;
        }

        public override void OnActiveEnter()
        {
            base.OnActiveEnter();
            Controller.RelativeVelocity = DiveVelocity;

            // Listen for collisions - need to bounce back up when we collide with the ground
            Controller.OnCollide.AddListener(OnCollide);
        }

        public void OnCollide(TerrainCastHit hit)
        {
            // Bounce only if it's the controller's bottom colliding with the floor
            if ((hit.Side & ControllerSide.Bottom) == 0) return;

            Controller.RelativeVelocity = BounceVelocity;
            Controller.OnCollide.RemoveListener(OnCollide);
            End();

            // Normally we can't use a double jump again until we attach to the floor, so make
            // it available manually
            Used = false;
        }
    }
}
