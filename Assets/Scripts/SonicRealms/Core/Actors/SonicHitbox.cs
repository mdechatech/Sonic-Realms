using System;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    [Serializable]
    public class EnemyEvent : UnityEvent<HealthSystem> { }

    /// <summary>
    /// A hitbox for standard interactions with badniks and other objects.
    /// </summary>
    public class SonicHitbox : Hitbox
    {
        /// <summary>
        /// The player's health system.
        /// </summary>
        [Tooltip("The player's health system.")]
        public HedgehogHealth Health;

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

        /// <summary>
        /// Whether the hitbox is vulnerable to enemy attack.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Whether the hitbox is vulnerable to enemy attack.")]
        public bool Vulnerable;

        /// <summary>
        /// Whether the hitbox will hurt badniks when it comes into contact.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Whether the hitbox will hurt badniks when it comes into contact.")]
        public bool Harmful;

        public override void Reset()
        {
            base.Reset();

            Health = Controller.GetComponent<HedgehogHealth>();
            Vulnerable = true;
            Harmful = false;
            OnKillEnemy = new EnemyEvent();

            HarmfulTag = LethalTag = "";
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

        public void NotifyEnemyKilled(HealthSystem enemy)
        {
            OnKillEnemy.Invoke(enemy);
            if(Health) Health.NotifyEnemyKilled(enemy);
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
