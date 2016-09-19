using System;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Generic platform that activates the object trigger when a controller collides with it.
    /// </summary>
    public class ActivatePlatform : ReactivePlatform
    {
        /// <summary>
        /// Whether to activate the platform when collided with.
        /// </summary>
        [Tooltip("Whether to activate the platform when collided with.")]
        public bool WhenColliding;

        /// <summary>
        /// Whether to activate the platform when a controller stands on it.
        /// </summary>
        [Tooltip("Whether to activate the platform when a controller stands on it.")]
        public bool WhenOnSurface;

        /// <summary>
        /// Whether to limit activation based on the angle of surface where the player collides.
        /// </summary>
        [Space]
        [FormerlySerializedAs("LimitAngle")]
        [Tooltip("Whether to limit activation based on the angle of surface where the player collides.")]
        public bool LimitSurfaceAngle;

        [AddIndentLevel]
        public LimitSurfaceAngleData LimitSurfaceAngleDetails;

        /// <summary>
        /// Whether to limit activation based on player velocity.
        /// </summary>
        [SetIndentLevel(0)]
        [Tooltip("Whether to limit activation based on player velocity.")]
        public bool LimitVelocity;

        [AddIndentLevel]
        public LimitVelocityData LimitVelocityDetails;

        /// <summary>
        /// Whether to limit activation based on player ground speed.
        /// </summary>
        [SetIndentLevel(0)]
        [Tooltip("Whether to limit activation based on player ground speed.")]
        public bool LimitGroundSpeed;

        [AddIndentLevel]
        public LimitGroundSpeedData LimitGroundSpeedDetails;

        /// <summary>
        /// Whether to limit activation based on player air speed.
        /// </summary>
        [SetIndentLevel(0)]
        [Tooltip("Whether to limit activation based on player air speed.")]
        public bool LimitAirSpeed;

        [AddIndentLevel]
        public LimitAirSpeedData LimitAirSpeedDetails;

        /// <summary>
        /// Whether to limit activation based on the player's current moves.
        /// </summary>
        [Tooltip("Whether to limit activation based on the player's current moves.")]
        [SetIndentLevel(0)]
        public bool LimitMoves;

        [AddIndentLevel]
        public LimitMovesData LimitMovesDetails;

        /// <summary>
        /// Whether to limit activation based on the player's current powerups.
        /// </summary>
        [SetIndentLevel(0)]
        [Tooltip("Whether to limit activation based on the player's current powerups.")]
        public bool LimitPowerups;

        [AddIndentLevel]
        public LimitPowerupsData LimitPowerupsDetails;

        public override void Reset()
        {
            base.Reset();
            if (!GetComponent<ObjectTrigger>()) gameObject.AddComponent<ObjectTrigger>();

            WhenColliding = false;
            WhenOnSurface = true;
        }

        public override void OnPlatformEnter(PlatformCollision contact)
        {
            if (!WhenColliding) return;
            if (!Check(contact.Latest.HitData)) return;

            ActivateObject(contact.Controller);
        }

        public override void OnPlatformStay(PlatformCollision collision)
        {
            if (!WhenColliding) return;

            if (Check(collision.Latest.HitData))
            {
                ActivateObject(collision.Controller);
                return;
            }

            DeactivateObject(collision.Controller);
        }

        public override void OnPlatformExit(PlatformCollision collision)
        {
            if (!WhenColliding) return;
            DeactivateObject(collision.Controller);
        }

        public bool Check(TerrainCastHit hit)
        {
            if (LimitSurfaceAngle && !LimitSurfaceAngleDetails.Allows(this, hit))
                return false;

            if (LimitVelocity && !LimitVelocityDetails.Allows(this, hit))
                return false;

            if (LimitGroundSpeed && !LimitGroundSpeedDetails.Allows(this, hit))
                return false;

            if (LimitAirSpeed && !LimitAirSpeedDetails.Allows(this, hit))
                return false;

            if (LimitMoves && !LimitMovesDetails.Allows(this, hit))
                return false;

            if (LimitPowerups && !LimitPowerupsDetails.Allows(this, hit))
                return false;

            return true;
        }

        public override void OnSurfaceEnter(SurfaceCollision collision)
        {
            if (!WhenOnSurface) return;
            if (!Check(collision.Latest.HitData)) return;

            ActivateObject(collision.Controller);
        }

        public override void OnSurfaceStay(SurfaceCollision collision)
        {
            if (!WhenOnSurface)
                return;

            if (Check(collision.Latest.HitData))
            {
                ActivateObject(collision.Controller);
            }
            else
            {
                DeactivateObject(collision.Controller);
            }
        }

        public override void OnSurfaceExit(SurfaceCollision collision)
        {
            if (!WhenOnSurface) return;
            DeactivateObject(collision.Controller);
        }

        public abstract class BaseLimiter
        {
            public abstract bool Allows(ActivatePlatform platform, TerrainCastHit hit);
        }

        [Serializable]
        public class LimitSurfaceAngleData : BaseLimiter
        {
            /// <summary>
            /// Whether to account for the object's rotation when checking angle.
            /// </summary>
            [Tooltip("Whether to account for the object's rotation when checking angle.")]
            public bool RelativeToRotation;

            /// <summary>
            /// Whether to account for player gravity when checking angle.
            /// </summary>
            [Tooltip("Whether to account for player gravity when checking angle.")]
            public bool RelativeToGravity;

            /// <summary>
            /// The minimum surface angle at which the platform activates when it is hit, in degrees.
            /// </summary>
            [Tooltip("The minimum surface angle at which the platform activates when hit, in degrees.")]
            public float SurfaceAngleMin;

            /// <summary>
            /// The maximum surface angle at which the platform activates when it is hit, in degrees.
            /// </summary>
            [Tooltip("The maximum surface angle at which the platform activates when it is hit, in degrees.")]
            public float SurfaceAngleMax;

            public override bool Allows(ActivatePlatform platform, TerrainCastHit hit)
            {
                var angle = hit.SurfaceAngle*Mathf.Rad2Deg;

                if (RelativeToRotation)
                    angle -= platform.transform.eulerAngles.z;

                if (RelativeToGravity)
                    angle = hit.Controller.RelativeAngle(angle);

                return DMath.AngleInRange_d(angle, SurfaceAngleMin, SurfaceAngleMax);
            }
        }

        [Serializable]
        public class LimitGroundSpeedData : BaseLimiter
        {
            /// <summary>
            /// If true, the values below will be compared to the absolute value of the player's ground velocity.
            /// </summary>
            [Tooltip("If true, the values below will be compared to the absolute value of the player's ground velocity.")]
            public bool UseAbsoluteValue;

            /// <summary>
            /// The player's minimum ground speed.
            /// </summary>
            [Tooltip("The player's minimum ground speed.")]
            public float Min;

            /// <summary>
            /// The player's maximum ground speed.
            /// </summary>
            [Tooltip("The player's maximum ground speed.")]
            public float Max;

            public override bool Allows(ActivatePlatform platform, TerrainCastHit hit)
            {
                if (!hit.Controller.Grounded)
                    return false;

                var speed = hit.Controller.GroundVelocity;

                if (UseAbsoluteValue)
                    speed = Mathf.Abs(speed);
                print("speed = " + speed + "; " + (speed >= Min && speed <= Max));
                return speed >= Min && speed <= Max;
            }
        }

        [Serializable]
        public class LimitAirSpeedData : BaseLimiter
        {
            /// <summary>
            /// If true, the values below will be compared to the absolute value of the player's air speed.
            /// </summary>
            [Tooltip("If true, the values below will be compared to the absolute value of the player's air speed.")]
            public bool UseAbsoluteValue;

            /// <summary>
            /// The player's minimum air speed.
            /// </summary>
            [Tooltip("The player's minimum air speed.")]
            public float Min;

            /// <summary>
            /// The player's maximum air speed.
            /// </summary>
            [Tooltip("The player's maximum air speed.")]
            public float Max;

            public override bool Allows(ActivatePlatform platform, TerrainCastHit hit)
            {
                if (hit.Controller.Grounded)
                    return false;

                var speed = hit.Controller.Velocity.magnitude;

                if (UseAbsoluteValue)
                    speed = Mathf.Abs(speed);

                return speed >= Min && speed <= Max;
            }
        }

        [Serializable]
        public class LimitVelocityData : BaseLimiter
        {
            /// <summary>
            /// If true, the values below will be compared to the player's velocity accounting for the object's rotation.
            /// </summary>
            [Tooltip("If true, the values below will be compared to the player's velocity accounting for the object's rotation.")]
            public bool RelativeToRotation;

            /// <summary>
            /// If true, the values below will be compared to the player's velocity relative to its gravity.
            /// </summary>
            [Tooltip("If checked, the values below will be compared to the player's velocity relative to its gravity.")]
            public bool RelativeToGravity;

            /// <summary>
            /// If true, the horizontal values below will be compared to the absolute value of the player's horizontal velocity.
            /// </summary>
            [Space]
            [Tooltip("If checked, the horizontal values below will be compared to the absolute value of the player's horizontal velocity.")]
            public bool UseAbsoluteHorizontal;

            /// <summary>
            /// The player's minimum horizontal speed.
            /// </summary>
            [Tooltip("The player's minimum horizontal speed.")]
            public float HorizontalMin;

            /// <summary>
            /// The player's maximum horizontal speed.
            /// </summary>
            [Tooltip("The player's maximum horizontal speed.")]
            public float HorizontalMax;

            /// <summary>
            /// If true, the vertical values below will be compared to the absolute value of the player's vertical velocity.
            /// </summary>
            [Space]
            [Tooltip("If checked, the vertical values below will be compared to the absolute value of the player's vertical velocity.")]
            public bool UseAbsoluteVertical;

            /// <summary>
            /// The player's minimum vertical speed.
            /// </summary>
            [Tooltip("The player's minimum vertical speed.")]
            public float VerticalMin;

            /// <summary>
            /// The player's maximum vertical speed.
            /// </summary>
            [Tooltip("The player's maximum vertical speed.")]
            public float VerticalMax;

            public override bool Allows(ActivatePlatform platform, TerrainCastHit hit)
            {
                var velocity = RelativeToGravity
                    ? hit.Controller.RelativeVelocity
                    : hit.Controller.Velocity;

                if (RelativeToRotation)
                    velocity = DMath.RotateBy(velocity, -platform.transform.eulerAngles.z*Mathf.Deg2Rad);

                var horizontal = UseAbsoluteHorizontal
                    ? Mathf.Abs(velocity.x)
                    : velocity.x;

                if (horizontal < HorizontalMin || horizontal > HorizontalMax)
                    return false;

                var vertical = UseAbsoluteVertical
                    ? Mathf.Abs(velocity.y)
                    : velocity.y;

                if (vertical < VerticalMin || vertical > VerticalMax)
                    return false;

                return true;
            }
        }

        [Serializable]
        public class LimitMovesData : BaseLimiter
        {
            /// <summary>
            /// If not empty, the name of the move the player must have in its Move Manager. The name is
            /// case-sensitive and without spaces, for example the Electric Special move would be referred
            /// to as "ElectricSpecial".
            /// </summary>
            [Tooltip("If not empty, the name of the move the player must have in its Move Manager. The name is " +
                     "case-sensitive and without spaces, for example the Electric Special move would be referred " +
                     "to as \"ElectricSpecial\".")]
            public string MustHaveMove;

            /// <summary>
            /// If not empty, the name of the move the player must be performing. The name is case-sensitive and
            /// without spaces, for example the Electric Special move would be referred to as "ElectricSpecial".
            /// </summary>
            [Tooltip("If not empty, the name of the move the player must be performing. The name is case-sensitive and " +
                     "without spaces, for example the Electric Special move would be referred to as \"ElectricSpecial\".")]
            public string MustPerformMove;

            /// <summary>
            /// If nonzero, the player must be performing a move on the given move layer. Move layer numbers can be found
            /// in the MoveLayer script.
            /// </summary>
            [Tooltip("If nonzero, the player must be performing a move on the given move layer. Move layer numbers can be found " +
                     "in the MoveLayer script.")]
            public int MustHaveMoveOnLayer;

            /// <summary>
            /// If nonzero, the player must not be performing a move on the given move layer. Move layer numbers can be found
            /// in the MoveLayer script.
            /// </summary>
            [Tooltip("If nonzero, the player must not be performing a move on the given move layer. Move layer numbers can be found " +
                     "in the MoveLayer script.")]
            public int MustNotHaveMoveOnLayer;

            public override bool Allows(ActivatePlatform platform, TerrainCastHit hit)
            {
                if (!string.IsNullOrEmpty(MustHaveMove) && !hit.Controller.HasMove(MustHaveMove))
                    return false;

                if (!string.IsNullOrEmpty(MustPerformMove) && !hit.Controller.IsPerforming(MustPerformMove))
                    return false;

                var moveManager = hit.Controller.GetMoveManager();

                if (MustHaveMoveOnLayer != 0 && (moveManager == null || moveManager[MustHaveMoveOnLayer] == null))
                    return false;

                if (MustNotHaveMoveOnLayer != 0 && (moveManager != null && moveManager[MustNotHaveMoveOnLayer] != null))
                    return false;

                return true;
            }
        }

        [Serializable]
        public class LimitPowerupsData : BaseLimiter
        {
            public string MustHavePowerup;

            public override bool Allows(ActivatePlatform platform, TerrainCastHit hit)
            {
                if (string.IsNullOrEmpty(MustHavePowerup))
                    return true;

                return hit.Controller.HasPowerup(MustHavePowerup);
            }
        }
    }
}
