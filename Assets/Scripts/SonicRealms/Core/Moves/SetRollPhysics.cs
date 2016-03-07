using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class SetRollPhysics : StateMachineBehaviour
    {
        [HideInInspector]
        public Roll Roll;

        /// <summary>
        /// Anything set to this will cause the value to stay unchanged.
        /// </summary>
        [Tooltip("Anything set to this will cause the value to stay unchanged.")]
        public float UnchangedValue;

        [Space]
        public float WidthChange;
        public float HeightChange,
            UphillGravity,
            DownhillGravity,
            Deceleration,
            Friction;

        public void Reset()
        {
            UnchangedValue =

            WidthChange =
            HeightChange =
            UphillGravity =
            DownhillGravity =
            Deceleration = 
            Friction = 0.0f;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Roll = Roll ?? animator.GetComponentInChildren<Roll>();
            if (Roll == null) return;

            if (WidthChange != UnchangedValue) Roll.WidthChange = WidthChange;
            if (HeightChange != UnchangedValue) Roll.HeightChange = HeightChange;
            if (UphillGravity != UnchangedValue) Roll.UphillGravity = UphillGravity;
            if (DownhillGravity != UnchangedValue) Roll.DownhillGravity = DownhillGravity;
            if (Deceleration != UnchangedValue) Roll.Deceleration = Deceleration;
            if (Friction != UnchangedValue) Roll.Friction = Friction;
        }
    }
}