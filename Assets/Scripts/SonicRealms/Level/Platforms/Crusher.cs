using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using SonicRealms.Level.Effects;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Platform that activates when the controller is being crushed. This happens when the controller
    /// finds a wall on both sides or touching the ground and ceiling on both its left and right sides.
    /// </summary>
    public class Crusher : ReactivePlatform
    {
        /// <summary>
        /// Whether to activate when being crushed on the left and right.
        /// </summary>
        [Tooltip("Whether to activate when being crushed on the left and right.")]
        public bool CrushHorizontally;

        /// <summary>
        /// How much to allow left and right walls to touch before crushing.
        /// </summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("How much to allow left and right walls to touch before crushing.")]
        public float HorizontalTolerance;

        /// <summary>
        /// Whether to activate when being crushed on the ground and ceiling.
        /// </summary>
        [Tooltip("Whether to activate when being crushed on the ground and ceiling.")]
        public bool CrushVertically;

        /// <summary>
        /// The fraction of the sensor test is checked each frame. This allows the crusher to work only
        /// if the object is moving down.
        /// </summary>
        private float _previousVerticalFraction;

        /// <summary>
        /// How much to allow grounds and ceilings to touch before crushing.
        /// </summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("How much to allow grounds and ceilings to touch before crushing.")]
        public float VerticalTolerance;

        public override void Reset()
        {
            // Add a kill player component by default - not mandatory though
            if (!GetComponent<KillPlayer>()) gameObject.AddComponent<KillPlayer>();

            CrushHorizontally = true;
            HorizontalTolerance = 0.0f;

            CrushVertically = true;
            VerticalTolerance = 0.5f;
        }

        public override void Awake()
        {
            base.Awake();
            _previousVerticalFraction = 1.0f;
        }

        /// <summary>
        /// Test used to see if a controller should be crushed.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public bool CheckCrush(HedgehogController controller)
        {
            return (CrushVertically && CheckVertical(controller)) ||
                   (CrushHorizontally && CheckHorizontal(controller));
        }

        /// <summary>
        /// Test used to see if a controller should be crushed vertically. It does not check for the
        /// CrushVertically flag.
        /// </summary>
        /// <param name="controller">The specified controller.</param>
        /// <returns></returns>
        public bool CheckVertical(HedgehogController controller)
        {
            if (!controller.Grounded || controller.LeftCeilingHit == null || controller.RightCeilingHit == null)
            {
                _previousVerticalFraction = 1.0f;
                return false;
            }

            var averageFraction = (controller.LeftCeilingHit.Hit.fraction + controller.RightCeilingHit.Hit.fraction)/
                                  2.0f;

            // If fraction is 1, the controller only started hitting the ceiling this frame, so excuse fraction checks
            if (_previousVerticalFraction == 1.0f) _previousVerticalFraction = averageFraction;

            // Check for fractions vs tolerances
            var result = controller.LeftCeilingHit.Hit.fraction <= 1.0f - VerticalTolerance &&
                         controller.RightCeilingHit.Hit.fraction <= 1.0f - VerticalTolerance;

            // The average fraction must also be less than the one last frame - this makes the check false
            // if the object away or stood still
            result &= (DMath.Equalsf(averageFraction) || averageFraction < _previousVerticalFraction - DMath.Epsilon);

            _previousVerticalFraction = averageFraction;

            return result;
        }

        /// <summary>
        /// Test used to see if a controller should be crushed horizontally. It does not check for the
        /// CrushHorizontally flag.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public bool CheckHorizontal(HedgehogController controller)
        {
            return controller.LeftWall != controller.RightWall && 

                    (controller.LeftWall != null &&
                    controller.LeftWallHit.Hit.fraction <= 1.0f - HorizontalTolerance) &&

                   (controller.RightWall != null &&
                    controller.RightWallHit.Hit.fraction <= 1.0f - HorizontalTolerance);
        }

        public override void OnPlatformStay(TerrainCastHit hit)
        {
            if (hit.Controller == null) return;

            if (CheckCrush(hit.Controller))
               TriggerObject(hit.Controller);
        }
    }
}
