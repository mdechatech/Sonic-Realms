using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Comes with events for being attacked by the player.
    /// </summary>
    [RequireComponent(typeof(AreaTrigger))]
    public class AttackTrigger : ReactiveArea
    {
        [Foldout("Events")]
        public AttackTriggerEvent OnAttack;

        protected List<Hitbox> Attacks;

        public override void Awake()
        {
            base.Awake();

            OnAttack = OnAttack ?? new AttackTriggerEvent();
            Attacks = new List<Hitbox>();
        }

        public bool HasHitbox(Hitbox hitbox)
        {
            return Attacks.Contains(hitbox);
        }

        public override bool IsInside(Hitbox hitbox)
        {
            return hitbox.IsAttackHitbox;
        }

        public override void OnAreaEnter(Hitbox hitbox)
        {
            Attacks.Add(hitbox);
            OnAttack.Invoke(hitbox);
        }

        public override void OnAreaExit(Hitbox hitbox)
        {
            Attacks.Remove(hitbox);
        }
    }
}
