using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Stores area collision info for AreaTriggers and ReactiveAreas.
    /// </summary>
    public struct AreaCollision
    {
        private readonly Contact[] _contacts;

        /// <summary>
        /// The controller that collided with the surface.
        /// </summary>
        public HedgehogController Controller { get { return Latest ? Latest.Controller : null; } }

        /// <summary>
        /// The number of contact points in the collision.
        /// </summary>
        public int Count { get { return _contacts != null ? _contacts.Length : 0; } }

        /// <summary>
        /// Gets the contact point at the given index. Use Count to find out how many contacts there are.
        /// </summary>
        /// <returns></returns>
        public Contact this[int index] { get { return _contacts[index]; } }

        /// <summary>
        /// The contact point that was registered last.
        /// </summary>
        public Contact Latest
        {
            get
            {
                return _contacts != null && _contacts.Length > 0
                    ? _contacts[_contacts.Length - 1]
                    : default(Contact);
            }
        }

        /// <summary>
        /// The latest contact between the controller's main hitbox and the are trigger, if any.
        /// </summary>
        public Contact Main
        {
            get
            {
                return _contacts == null
                    ? default(Contact)
                    : _contacts.FirstOrDefault(contact => contact.Hitbox.IsMainHitbox);
            }
        }

        /// <summary>
        /// The latest contact between the controller's attack hitbox and the area trigger, if any.
        /// </summary>
        public Contact Attack
        {
            get
            {
                return _contacts == null
                    ? default(Contact)
                    : _contacts.FirstOrDefault(contact => contact.Hitbox.IsAttackHitbox);
            }
        }

        /// <summary>
        /// The latest contact that didn't involve the player's main or attack hitbox, if any.
        /// </summary>
        public Contact Other
        {
            get
            {
                return _contacts == null
                    ? default(Contact)
                    : _contacts.FirstOrDefault(
                        contact => !contact.Hitbox.IsAttackHitbox && !contact.Hitbox.IsMainHitbox);
            }
        }

        public AreaCollision(IEnumerable<Contact> contacts)
        {
            _contacts = contacts.ToArray();
        }

        public override string ToString()
        {
            return
                string.Format(
                    "[AreaCollision Contacts=[{0}], Controller={1}, Latest={2}, Main={3}, Attack={4}, Other={5}]",
                    string.Join(", ", _contacts.Select(c => c.ToString()).ToArray()), Controller, Latest, Main, Attack,
                    Other);
        }

        /// <summary>
        /// Returns true if the collision has any contacts, false otherwise.
        /// </summary>
        public static implicit operator bool (AreaCollision collision)
        {
            return collision._contacts != null && collision._contacts.Length > 0;
        }

        /// <summary>
        /// A single instance of area collision, of which many may make up AreaCollision.
        /// </summary>
        public struct Contact
        {
            /// <summary>
            /// The area trigger that collided with the hitbox.
            /// </summary>
            public readonly AreaTrigger AreaTrigger;

            /// <summary>
            /// The hitbox that collided with the area trigger.
            /// </summary>
            public readonly Hitbox Hitbox;

            /// <summary>
            /// The owner of the hitbox that collided with the area trigger.
            /// </summary>
            public HedgehogController Controller { get { return Hitbox ? Hitbox.Controller : null; } }

            /// <summary>
            /// The collider of the area trigger that collided with the hitbox.
            /// </summary>
            public Collider2D AreaCollider
            {
                get { return AreaTrigger ? AreaTrigger.GetComponent<Collider2D>() : null; }
            }

            /// <summary>
            /// The position of the hitbox at the time of collision with the area trigger.
            /// </summary>
            public readonly Vector2 HitboxPosition;

            /// <summary>
            /// The position of the area trigger at the time of collision with the hitbox.
            /// </summary>
            public readonly Vector2 AreaPosition;

            /// <summary>
            /// The controller's velocity at the time of contact.
            /// </summary>
            public readonly Vector2 Velocity;

            /// <summary>
            /// The controller's velocity relative to its gravity at the time of contact.
            /// </summary>
            public readonly Vector2 RelativeVelocity;

            /// <summary>
            /// The controller's ground speed at the time of contact.
            /// </summary>
            public readonly float GroundVelocity;

            /// <summary>
            /// The controller's surface angle at the time of contact.
            /// </summary>
            public readonly float SurfaceAngle;

            /// <summary>
            /// The controller's surface angle relative to its gravity at the time of contact.
            /// </summary>
            public readonly float RelativeSurfaceAngle;

            public Contact(AreaTrigger areaTrigger, Hitbox hitbox)
                : this(areaTrigger, hitbox, areaTrigger.transform.position, hitbox.transform.position)
            {

            }

            public Contact(AreaTrigger areaTrigger, Hitbox hitbox, Vector2 areaPosition, Vector2 hitboxPosition)
            {
                AreaTrigger = areaTrigger;
                Hitbox = hitbox;
                HitboxPosition = hitboxPosition;
                AreaPosition = areaPosition;

                Velocity = hitbox.Controller.Velocity;
                RelativeVelocity = hitbox.Controller.RelativeVelocity;
                GroundVelocity = hitbox.Controller.GroundVelocity;
                SurfaceAngle = hitbox.Controller.SurfaceAngle;
                RelativeSurfaceAngle = hitbox.Controller.RelativeSurfaceAngle;
            }

            public override string ToString()
            {
                return
                    string.Format(
                        "[Contact Hitbox={0}, AreaTrigger={1}, HitboxPosition={2}, AreaPosition={3}, Velocity={4}, " +
                        "RelativeVelocity={5}, GroundVelocity={6}, SurfaceAngle={7}, RelativeSurfaceAngle={8}, " +
                        "Controller={9}]",
                        Hitbox, AreaTrigger, HitboxPosition, AreaPosition, Velocity, RelativeVelocity, GroundVelocity,
                        SurfaceAngle, RelativeSurfaceAngle, Controller);
            }

            /// <summary>
            /// Returns true if the contact has any data, false otherwise.
            /// </summary>
            public static implicit operator bool (Contact contact)
            {
                return contact.Hitbox != null;
            }
        }
    }
}
