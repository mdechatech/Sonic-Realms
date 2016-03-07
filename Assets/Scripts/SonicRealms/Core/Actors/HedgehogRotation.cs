using System;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Controls sprite rotation based on the controller's state.
    /// </summary>
    public class HedgehogRotation : MonoBehaviour
    {
        public HedgehogController Controller;
        public Transform RendererTransform;

        [NonSerialized]
        public float TrueRotation;
        public float Rotation
        {
            get { return RendererTransform.eulerAngles.z; }
            set
            {
                RendererTransform.eulerAngles = new Vector3(
                    RendererTransform.eulerAngles.x,
                    RendererTransform.eulerAngles.y,
                    value);
            }
        }

        /// <summary>
        /// Whether to rotate to the direction of gravity when not rotating to surface angle.
        /// </summary>
        [Tooltip("Whether to rotate to the direction of gravity when not rotating to surface angle.")]
        public bool RotateToGravity;

        /// <summary>
        /// Whether to rotate to surface angle during a roll.
        /// </summary>
        [Tooltip("Whether to rotate to surface angle during a roll.")]
        public bool RotateDuringRoll;

        /// <summary>
        /// Whether to rotate to surface angle when standing.
        /// </summary>
        [Tooltip("Whether to rotate to surface angle when standing.")]
        public bool RotateDuringStand;

        /// <summary>
        /// How quickly the controller rotates back to normal after leaving the ground, in degrees per second.
        /// </summary>
        [Tooltip("How quickly the controller rotates back to normal after leaving the ground, in degrees per second.")]
        public float AirRecoveryRate;
        protected float AirRotation;

        /// <summary>
        /// Rotates to surface angle if it is this much different than the direction of gravity, in degrees.
        /// Makes it so the controller doesn't rotate when the ground is flat.
        /// </summary>
        [Tooltip("Rotates to surface angle if it is this much different than the direction of gravity, in degrees. " +
            "Makes it so the controller doesn't rotate when the ground is flat.")]
        public float MinimumAngle;

        /// <summary>
        /// How many orientations the sprite has. For example, if precision is 8, the sprite will rotate in
        /// 360/8 = 45 degree intervals.
        /// </summary>
        [Tooltip("How many orientations the sprite has. For example, if precision is 8, the sprite will rotate " +
                 "in 360/8 = 45 degree intervals.")]
        public float Precision;

        protected Roll Roll;

        public void Reset()
        {
            RotateToGravity = true;
            RotateDuringRoll = false;
            RotateDuringStand = false;
            MinimumAngle = 22.5f;
            AirRecoveryRate = 360.0f;
            Precision = 45.0f;

            Controller = GetComponentInParent<HedgehogController>();
            if (Controller == null) return;
            RendererTransform = Controller.RendererObject.transform;
        }

        public void Awake()
        {
            TrueRotation = 0.0f;
        }

        public void Start()
        {
            RendererTransform = RendererTransform ?? Controller.RendererObject.transform;
            Roll = Controller.GetComponent<MoveManager>() ? Controller.GetComponent<MoveManager>().Get<Roll>() : null;
        }

        public void Update()
        {
            if (Controller.Grounded)
            {
                var rotateToSensors = true;
                rotateToSensors &= RotateDuringStand || !DMath.Equalsf(Controller.GroundVelocity);
                rotateToSensors &= Roll == null || RotateDuringRoll || !Roll.Active;
                rotateToSensors &=
                    Mathf.Abs(DMath.ShortestArc_d(Controller.SensorsRotation, Controller.GravityRight)) >
                    MinimumAngle;

                TrueRotation = rotateToSensors ? Controller.SensorsRotation : Controller.GravityRight;
            }
            else
            {
                if ((RotateDuringStand || !DMath.Equalsf(Controller.GroundVelocity)) &&
                    (Roll == null || RotateDuringRoll || !Roll.Active))
                {
                    var difference = DMath.ShortestArc_d(Rotation, Controller.GravityRight);
                    difference = difference > 0.0f
                        ? Mathf.Min(AirRecoveryRate * Time.deltaTime, difference)
                        : Mathf.Max(-AirRecoveryRate * Time.deltaTime, difference);

                    TrueRotation += difference;
                }
                else
                {
                    TrueRotation = Controller.GravityRight;
                }
            }

            FixRotation();
        }

        /// <summary>
        /// Rounds rotation to the nearest precision.
        /// </summary>
        public void FixRotation()
        {
            Rotation = DMath.Round(TrueRotation, 360.0f/Precision, Controller.GravityRight);
        }
    }
}
