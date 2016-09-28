using UnityEngine;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;

namespace SonicRealms.Level.Objects
{
    public class Checkpoint : ReactiveEffect
    {
        [Foldout("Animation")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when the checkpoint is immediately activated.
        /// </summary>
        [Foldout("Animation")]
        public string ActivateImmediateTrigger;
        protected int ActivateImmediateTriggerHash;

        [Foldout("Debug")]
        public bool ActivatedImmediately;

        public override void Reset()
        {
            base.Reset();

            Animator = GetComponent<Animator>();
        }

        public override void Awake()
        {
            base.Awake();

            Animator = Animator ?? GetComponent<Animator>();
            ActivateImmediateTriggerHash = Animator.StringToHash(ActivateImmediateTrigger);
        }

        public override void OnActivate(HedgehogController controller)
        {
            if (GameManager.Instance == null) return;

            if (!ActivatedImmediately)
            {
                var level = GameManager.Instance.Level as GoalLevelManager;
                if (level != null)
                {
                    level.Checkpoint = gameObject;
                    GameManager.Instance.SaveProgress();
                }
            }

            EffectTrigger.enabled = false;
        }

        /// <summary>
        /// Use this to turn on the checkpoint when the level starts.
        /// </summary>
        public virtual void ActivateImmediate()
        {
            ActivatedImmediately = true;
            EffectTrigger.enabled = false;

            if (Animator && ActivateImmediateTriggerHash != 0)
                Animator.SetTrigger(ActivateImmediateTriggerHash);
        }
    }
}
