using System;
using Hedgehog.Core.Triggers;
using Hedgehog.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Actors
{
    [Serializable]
    public class EnemyEvent : UnityEvent<HealthSystem> { }

    public class SonicHitbox : Hitbox
    {
        public HedgehogHealth Health;
        public bool Vulnerable;
        public bool Harmful;

        /// <summary>
        /// If not Untagged, colliders with these tags will be harmful to the player.
        /// </summary>
        [Tag, Foldout("Collision")]
        [Tooltip("If not Untagged, colliders with these tags will be harmful to the player.")]
        public string HarmfulTag;
        protected bool CheckHarmfulTag;

        /// <summary>
        /// If not Untagged, colliders with these tags will be lethal to the player.
        /// </summary>
        [Tag, Foldout("Collision")]
        [Tooltip("If not Untagged, colliders with these tags will be lethal to the player.")]
        public string LethalTag;
        protected bool CheckLethalTag;

        /// <summary>
        /// Invoked when this hitbox kills an enemy.
        /// </summary>
        [Foldout("Events")]
        public EnemyEvent OnKillEnemy;

        public override void Reset()
        {
            base.Reset();

            Health = Controller.GetComponent<HedgehogHealth>();
            Vulnerable = true;
            Harmful = false;
            OnKillEnemy = new EnemyEvent();

            HarmfulTag = LethalTag = "Untagged";
        }

        public override void Awake()
        {
            base.Awake();
            OnKillEnemy = OnKillEnemy ?? new EnemyEvent();
        }

        public void Start()
        {
            CheckHarmfulTag = HarmfulTag != "Untagged";
            CheckLethalTag = LethalTag != "Untagged";
        }

        public override bool AllowCollision(AreaTrigger trigger)
        {
            return true;
        }

        public void NotifyEnemyKilled(HealthSystem enemy)
        {
            OnKillEnemy.Invoke(enemy);
        }

        public void CheckTags(Component component)
        {
            if(CheckHarmfulTag && component.CompareTag(HarmfulTag)) Health.TakeDamage(component.transform);
            if(CheckLethalTag && component.CompareTag(LethalTag)) Health.Kill(component.transform);
        }

        public override void OnHitboxStay(AreaTrigger trigger)
        {
            CheckTags(trigger);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            CheckTags(other);
        }
    }
}
