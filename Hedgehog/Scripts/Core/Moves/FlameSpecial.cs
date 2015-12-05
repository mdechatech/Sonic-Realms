using UnityEngine;

namespace Hedgehog.Core.Moves
{
    /// <summary>
    /// The air dash that Sonic can perform when he has a flame shield.
    /// </summary>
    public class FlameSpecial : DoubleJumpMove
    {
        /// <summary>
        /// The velocity of the dash, in units per second.
        /// </summary>
        [Tooltip("The velocity of the dash, in units per second.")]
        public Vector2 DashVelocity;

        /// <summary>
        /// Keeps track of whether we need to flip the sprite when performing the dash.
        /// </summary>
        private bool _flipped;

        public override void Reset()
        {
            base.Reset();
            DashVelocity = new Vector2(4.8f, 0.0f);
        }

        public override void OnActiveEnter(State previousState)
        {
            base.OnActiveEnter(previousState);

            // Dash based on what direction we're facing
            if (Controller.FacingForward)
                Controller.RelativeVelocity = DashVelocity;
            else
                Controller.RelativeVelocity = new Vector2(-DashVelocity.x, DashVelocity.y);

            End();

            // Store whether or not we flipped the sprite so we can flip back later
            _flipped = !Controller.FacingForward;
            if (_flipped)
                transform.localScale -= new Vector3(transform.localScale.x * 2.0f, 0.0f);

            // Going to flip the sprite back when we land
            Controller.OnAttach.AddListener(OnAttach);
        }

        public void OnAttach()
        {
            // If we flipped during the dash then flip the sprite back
            if (_flipped)
                transform.localScale -= new Vector3(transform.localScale.x * 2.0f, 0.0f);
            Controller.OnAttach.RemoveListener(OnAttach);
        }
    }
}
