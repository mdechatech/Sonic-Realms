using Hedgehog.Core.Actors;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Core.Moves
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
        public string DistanceFloat;
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
        }

        public override void Start()
        {
            base.Start();
            CreateEdgeSensors();
        }

        public void OnDestroy()
        {
            Destroy(EdgeSensorLeft.gameObject);
            Destroy(EdgeSensorRight.gameObject);
        }

        protected virtual void CreateEdgeSensors()
        {
            EdgeSensorLeft = new GameObject {name = "Edge Balance Left"}.transform;
            EdgeSensorRight = new GameObject {name = "Edge Balance Right"}.transform;

            EdgeSensorLeft.SetParent(Controller.Sensors.transform);
            EdgeSensorRight.SetParent(Controller.Sensors.transform);

            EdgeSensorLeft.position = Controller.Sensors.BottomLeft.position + Vector3.down*DMath.Epsilon;
            EdgeSensorRight.position = Controller.Sensors.BottomRight.position + Vector3.down*DMath.Epsilon;
        }

        public override void SetAnimatorParameters()
        {
            base.SetAnimatorParameters();
            if(!string.IsNullOrEmpty(DistanceFloat))
                Animator.SetFloat(DistanceFloat, EdgeDistance);
        }

        public override bool Available()
        {
            return Controller.Grounded && 
                DMath.Equalsf(Controller.GroundVelocity) &&
                Controller.SecondarySurface == null;
        }

        public override bool InputActivate()
        {
            if (CheckEdge() < MaxDistance)
                return true;

            End();
            return false;
        }

        public float CheckEdge()
        {
            TerrainCastHit hit;
            float distance = 0.0f;

            if (Controller.Footing == Footing.Left)
            {
                hit = Controller.TerrainCast(EdgeSensorRight.position, EdgeSensorLeft.position, ControllerSide.Bottom);
                distance = Vector2.Distance(hit.Hit.point, EdgeSensorRight.position);
            }
            else if (Controller.Footing == Footing.Right)
            {
                hit = Controller.TerrainCast(EdgeSensorLeft.position, EdgeSensorRight.position, ControllerSide.Bottom);
                distance = Vector2.Distance(hit.Hit.point, EdgeSensorLeft.position);
            }

            distance = Vector2.Distance(EdgeSensorLeft.position, EdgeSensorRight.position) - distance;
            EdgeDistance = distance;

            return distance;
        }

        public override bool InputDeactivate()
        {
            return CheckEdge() > MaxDistance || !Available();
        }

        public override void OnActiveEnter(State previousState)
        {
            if (!AllowDuck)
            {
                var duck = Manager.Get<Duck>();
                if (duck != null)
                    duck.InputEnabled = false;
            }

            if (!AllowLookUp)
            {
                var lookUp = Manager.Get<LookUp>();
                if (lookUp != null)
                    lookUp.InputEnabled = false;
            }
        }

        public override void OnActiveUpdate()
        {
            Controller.FacingForward = Controller.Footing == Footing.Left;
        }

        public override void OnActiveExit()
        {
            if (!AllowDuck)
            {
                var duck = Manager.Get<Duck>();
                if (duck != null)
                    duck.InputEnabled = true;
            }

            if (!AllowLookUp)
            {
                var lookUp = Manager.Get<LookUp>();
                if (lookUp != null)
                    lookUp.InputEnabled = true;
            }
        }
    }
}
