using SonicRealms.Core.Actors;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Activates when the controller is a certain distance off the edge of a platform.
    /// </summary>
    public class EdgeBalance : Move
    {
        #region Control
        /// <summary>
        /// Maximum distance between the grounded sensor and the edge of the platform to activate, in units.
        /// </summary>
        [Tooltip("Maximum distance between the grounded sensor and the edge of the platform to activate, in units.")]
        public float MaxDistance;

        /// <summary>
        /// Whether to allow ducking while active.
        /// </summary>
        [Tooltip("Whether to allow ducking while active.")]
        public bool AllowDuck;

        /// <summary>
        /// Whether to allow looking up while active.
        /// </summary>
        [Tooltip("Whether to allow looking up while active.")]
        public bool AllowLookUp;
        #endregion
        #region Animation
        /// <summary>
        /// Name of an Animator float set to the distance between the sensor without footing and the edge
        /// of the platform, or 0 if inactive.
        /// </summary>
        [AnimationFoldout]
        [Tooltip("Name of an Animator float set to the distance between the sensor without footing and the edge " +
                 "of the platform, or 0 if inactive.")]
        public string DistanceFloat;
        protected int DistanceFloatHash;
        #endregion

        /// <summary>
        /// Current distance from the edge.
        /// </summary>
        [Tooltip("Current distance from the edge.")]
        public float EdgeDistance;

        protected Transform EdgeSensorLeft;
        protected Transform EdgeSensorRight;

        public override void Reset()
        {
            base.Reset();
            MaxDistance = 0.1f; // s3k third animation is 0.04
            AllowDuck = true;
            AllowLookUp = false;
        }

        public override void Awake()
        {
            base.Awake();
            EdgeDistance = 0.0f;
            DistanceFloatHash = string.IsNullOrEmpty(DistanceFloat) ? 0 : Animator.StringToHash(DistanceFloat);
        }

        public override void OnManagerAdd()
        {
            CreateEdgeSensors();
            if (Animator != null && DistanceFloatHash != 0)
                Animator.SetFloat(DistanceFloatHash, 0f);
        }

        public void OnDestroy()
        {
            if(EdgeSensorLeft != null) Destroy(EdgeSensorLeft.gameObject);
            if(EdgeSensorRight != null) Destroy(EdgeSensorRight.gameObject);
        }

        protected virtual void CreateEdgeSensors()
        {
            EdgeSensorLeft = new GameObject {name = "Edge Balance Left"}.transform;
            EdgeSensorRight = new GameObject {name = "Edge Balance Right"}.transform;

            EdgeSensorLeft.SetParent(Controller.Sensors.transform);
            EdgeSensorRight.SetParent(Controller.Sensors.transform);

            EdgeSensorLeft.position = Controller.Sensors.BottomLeft.position + Vector3.down*SrMath.Epsilon;
            EdgeSensorRight.position = Controller.Sensors.BottomRight.position + Vector3.down*SrMath.Epsilon;
        }

        public override void SetAnimatorParameters()
        {
            base.SetAnimatorParameters();
            if(DistanceFloatHash != 0)
                Animator.SetFloat(DistanceFloatHash, EdgeDistance);
        }

        public override bool Available
        {
            get
            {
                return Controller.Grounded &&
                       Controller.SecondarySurfaceHit == null;
            }
        }

        public override bool ShouldPerform
        {
            get
            {
                if (CheckEdge() < MaxDistance)
                    return true;

                End();
                return false;
            }
        }

        public float CheckEdge()
        {
            TerrainCastHit hit;
            float distance = EdgeDistance = float.MaxValue;

            if (Controller.Side == GroundSensorType.Left)
            {
                hit = Controller.TerrainCast(EdgeSensorRight.position, EdgeSensorLeft.position, ControllerSide.Bottom);
                if (hit == null)
                {
                    if (Animator != null && DistanceFloatHash != 0)
                        Animator.SetFloat(DistanceFloatHash, EdgeDistance);
                    return distance;
                }

                distance = Vector2.Distance(hit.Raycast.point, EdgeSensorRight.position);
            }
            else if (Controller.Side == GroundSensorType.Right)
            {
                hit = Controller.TerrainCast(EdgeSensorLeft.position, EdgeSensorRight.position, ControllerSide.Bottom);
                if (hit == null)
                {
                    if (Animator != null && DistanceFloatHash != 0)
                        Animator.SetFloat(DistanceFloatHash, EdgeDistance);
                    return distance;
                }

                distance = Vector2.Distance(hit.Raycast.point, EdgeSensorLeft.position);
            }

            distance = Vector2.Distance(EdgeSensorLeft.position, EdgeSensorRight.position) - distance;
            EdgeDistance = distance;

            if (Animator != null && DistanceFloatHash != 0)
                Animator.SetFloat(DistanceFloatHash, EdgeDistance);

            return distance;
        }

        public override bool ShouldEnd
        {
            get { return CheckEdge() > MaxDistance || !Available; }
        }

        public override void OnActiveEnter(State previousState)
        {
            if (SrMath.Equalsf(Controller.GroundVelocity))
                Controller.IsFacingForward = Controller.Side == GroundSensorType.Left;

            if (!AllowDuck)
            {
                var duck = Manager.Get<Duck>();
                if (duck != null)
                    duck.CheckShouldPerform = false;
            }

            if (!AllowLookUp)
            {
                var lookUp = Manager.Get<LookUp>();
                if (lookUp != null)
                    lookUp.CheckShouldPerform = false;
            }
        }

        public override void OnActiveUpdate()
        {
            if(SrMath.Equalsf(Controller.GroundVelocity))
                Controller.IsFacingForward = Controller.Side == GroundSensorType.Left;
        }

        public override void OnActiveExit()
        {
            if (!AllowDuck)
            {
                var duck = Manager.Get<Duck>();
                if (duck != null)
                    duck.CheckShouldPerform = true;
            }

            if (!AllowLookUp)
            {
                var lookUp = Manager.Get<LookUp>();
                if (lookUp != null)
                    lookUp.CheckShouldPerform = true;
            }
        }
    }
}
