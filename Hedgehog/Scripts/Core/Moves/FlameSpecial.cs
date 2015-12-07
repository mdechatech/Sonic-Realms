using Hedgehog.Level.Areas;
using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// The air dash that Sonic can perform when he has a flame shield.
    /// </summary>
    public class FlameSpecial : DoubleJump
    {
        /// <summary>
        /// The velocity of the dash, in units per second.
        /// </summary>
        [Tooltip("The velocity of the dash, in units per second.")]
        public Vector2 DashVelocity;

        public override void Reset()
        {
            base.Reset();
            DashVelocity = new Vector2(4.8f, 0.0f);
        }

        public override void OnActiveEnter()
        {
            base.OnActiveEnter();

            // Dash based on what direction we're facing
            if (Controller.FacingForward)
                Controller.RelativeVelocity = DashVelocity;
            else
                Controller.RelativeVelocity = new Vector2(-DashVelocity.x, DashVelocity.y);

            End();

            // Flip the shield based on which way we're facing
            if ((!Controller.FacingForward && transform.localScale.x > 0.0f) ||
                (Controller.FacingForward && transform.localScale.x < 0.0f))
                transform.localScale -= new Vector3(transform.localScale.x*2.0f, 0.0f);
        }
    }
}
