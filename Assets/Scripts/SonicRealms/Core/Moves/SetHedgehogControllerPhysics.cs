using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class SetHedgehogControllerPhysics : StateMachineBehaviour
    {
        [HideInInspector]
        public HedgehogController Controller;

        /// <summary>
        /// Anything set to this will cause the value to stay unchanged.
        /// </summary>
        [Tooltip("Anything set to this will cause the value to stay unchanged.")]
        public float UnchangedValue;
        [Space]
        public float GroundFriction;
        public float 
            GravityDirection,
            AirGravity,
            AirDrag,
            SlopeGravity;

        public void Reset()
        {
            UnchangedValue      =

            GroundFriction      = 
            GravityDirection    = 
            AirGravity          = 
            SlopeGravity        = 0.0f;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Controller = Controller ?? animator.GetComponentInParent<HedgehogController>();
            if (Controller == null) return;

            if (GroundFriction != UnchangedValue) Controller.GroundFriction = GroundFriction;
            if (GravityDirection != UnchangedValue) Controller.GravityDirection = GravityDirection;
            if (AirGravity != UnchangedValue) Controller.AirGravity = AirGravity;
            if (AirDrag != UnchangedValue) Controller.AirDrag = AirDrag;
            if (SlopeGravity != UnchangedValue) Controller.SlopeGravity = SlopeGravity;
        }
    }
}
