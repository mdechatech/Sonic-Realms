using System.Collections;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    public class LevelGoal : ReactiveObject
    {
        /// <summary>
        /// How many seconds to wait before ending the level.
        /// </summary>
        [Tooltip("How many seconds to wait before ending the level.")]
        public float Delay;

        /// <summary>
        /// The object to focus on at the end of the level.
        /// </summary>
        [Tooltip("The object to focus on at the end of the level.")]
        public Transform FocusTarget;

        /// <summary>
        /// The speed at which to focus on the object. Make it slow to give it a nice pan.
        /// </summary>
        [Tooltip("The speed at which to focus on the object. Make it slow to give it a nice pan.")]
        public Vector2 FocusSpeed;

        public override void OnActivate(HedgehogController controller)
        {
            var camera = controller.GetCamera();
            if(camera != null) camera.FinishLevel(FocusTarget, FocusSpeed);

            StartCoroutine(DelayedFinish());
        }

        protected IEnumerator DelayedFinish()
        {
            yield return new WaitForSeconds(Delay);
            Finish();
        }

        public virtual void Finish()
        {
            var level = GameManager.Instance.Level as GoalLevelManager;
            if (level == null) return;

            level.FinishLevel();
        }
    }
}
