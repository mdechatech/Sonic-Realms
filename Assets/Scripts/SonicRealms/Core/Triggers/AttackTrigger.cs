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

        public override bool CanTouch(AreaCollision.Contact contact)
        {
            return contact.Hitbox.IsAttackHitbox;
        }

        public override void OnAreaEnter(AreaCollision collision)
        {
            Attacks.Add(collision.Latest.Hitbox);
            OnAttack.Invoke(collision.Latest.Hitbox);
        }

        public override void OnAreaExit(AreaCollision collision)
        {
            Attacks.Remove(collision.Latest.Hitbox);
        }
    }
}
