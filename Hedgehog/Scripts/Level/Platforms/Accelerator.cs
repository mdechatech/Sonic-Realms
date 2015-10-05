using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level.Platforms
{
    /// <summary>
    /// Accelerates or decelerates a controller when on its surface.
    /// </summary>
    public class Accelerator : ReactivePlatform
    {
        /// <summary>
        /// The acceleration, in units per second squared. Use negative values for deceleration.
        /// </summary>
        [SerializeField, Tooltip("Acceleration in units per second squared.")]
        public float Acceleration;

        /// <summary>
        /// The target velocity, in units per second.
        /// </summary>
        [SerializeField, Tooltip("Target velocity in units per second.")]
        public float TargetVelocity;

        /// <summary>
        /// Whether to make the controller faster regardless of which way it's facing.
        /// </summary>
        [SerializeField, Tooltip("Whether to go faster regardless of which way the controller is facing.")]
        public bool AccelerateBothWays;

        /// <summary>
        /// Whether to cancel out the controller's deceleration before beginning acceleration.
        /// </summary>
        [SerializeField, Tooltip("Whether to account for friction.")]
        public bool AccountForFriction;

        public void Reset()
        {
            Acceleration = 5.0f;
            TargetVelocity = 5.0f;
            AccelerateBothWays = false;
            AccountForFriction = true;
        }

        public override void OnSurfaceStay(HedgehogController controller, TerrainCastHit hit)
        {
            var oldVelocity = controller.GroundVelocity;

            if (AccelerateBothWays)
            {
                if (AccountForFriction)
                    controller.GroundVelocity += (controller.GroundDeceleration + Acceleration)
                                                 *Mathf.Sign(controller.GroundVelocity)*Time.fixedDeltaTime;
                else
                    controller.GroundVelocity += Acceleration*Mathf.Sign(controller.GroundVelocity)*Time.fixedDeltaTime;
                
                var absTargetVg = Mathf.Abs(TargetVelocity);
                if (Mathf.Abs(oldVelocity) < absTargetVg && Mathf.Abs(controller.GroundVelocity) > absTargetVg)
                {
                        controller.GroundVelocity = TargetVelocity*Mathf.Sign(controller.GroundVelocity);
                }
            }
            else
            {
                if (AccountForFriction)
                    controller.GroundVelocity += (Acceleration + controller.GroundDeceleration*Mathf.Sign(Acceleration))
                                                 *Time.fixedDeltaTime;
                else
                    controller.GroundVelocity += Acceleration*Time.fixedDeltaTime;

                if (TargetVelocity > 0.0f)
                {
                    if (oldVelocity < TargetVelocity && controller.GroundVelocity > TargetVelocity)
                        controller.GroundVelocity = TargetVelocity;
                }
                else
                {
                    if (oldVelocity > TargetVelocity && controller.GroundVelocity < TargetVelocity)
                        controller.GroundVelocity = TargetVelocity;
                }
            }
            
        }
    }
}
