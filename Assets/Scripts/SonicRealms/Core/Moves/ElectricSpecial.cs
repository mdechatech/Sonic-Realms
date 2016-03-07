using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// The double jump that Sonic can perform when he has an electric shield.
    /// </summary>
    public class ElectricSpecial : DoubleJump
    {
        /// <summary>
        /// The vertical velocity of the special move, in units per second.
        /// </summary>
        [Tooltip("The vertical velocity of the special move, in units per second.")]
        public float Velocity;

        public override void Reset()
        {
            base.Reset();
            Velocity = 3.3f;
        }

        public override void OnActiveEnter()
        {
            base.OnActiveEnter();
            Controller.RelativeVelocity = new Vector2(Controller.RelativeVelocity.x, Velocity);
            End();
        }
    }
}
