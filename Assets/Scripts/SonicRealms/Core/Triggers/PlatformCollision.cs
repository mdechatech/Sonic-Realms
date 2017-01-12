using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Triggers
{
    /// <summary>
    /// Stores platform collision info for use with PlatformTriggers and ReactivePlatforms.
    /// </summary>
    public struct PlatformCollision
    {
        private readonly Contact[] _contacts;

        /// <summary>
        /// The controller that collided with the surface.
        /// </summary>
        public HedgehogController Controller
        {
            get { return _contacts.Length > 0 ? _contacts[0].HitData.Controller : null; }
        }

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
        /// Gets the latest contact point that contains the given sensor. Passing in SensorType.All will
        /// return the latest contact.
        /// </summary>
        public Contact this[SensorType sensor]
        {
            get
            {
                return _contacts == null
                    ? default(Contact)
                    : _contacts.FirstOrDefault(contact => sensor.Has(contact.Sensor));
            }
        }

        /// <summary>
        /// The contact point that was registered last.
        /// </summary>
        public Contact Latest { get { return this[SensorType.All]; } }

        /// <summary>
        /// The latest contact between the controller's left ground sensor and the surface, if any.
        /// </summary>
        public Contact BottomLeft { get { return this[SensorType.BottomLeft]; } }

        /// <summary>
        /// The latest contact between the controller's right ground sensor and the surface, if any.
        /// </summary>
        public Contact BottomRight { get { return this[SensorType.BottomRight]; } }
        
        /// <summary>
        /// The latest contact between the controller's left side sensor and the surface, if any.
        /// </summary>
        public Contact CenterLeft { get { return this[SensorType.CenterLeft]; } }

        /// <summary>
        /// The latest contact between the controller's right side sensor and the surface, if any.
        /// </summary>
        public Contact CenterRight { get { return this[SensorType.CenterRight]; } }
        
        /// <summary>
        /// The latest contact between the controller's left ceiling sensor and the surface, if any.
        /// </summary>
        public Contact TopLeft { get { return this[SensorType.TopLeft]; } }

        /// <summary>
        /// The latest contact between the controller's right ceiling sensor and the surface, if any.
        /// </summary>
        public Contact TopRight { get { return this[SensorType.TopRight]; } }

        public PlatformCollision(IEnumerable<Contact> contacts)
        {
            _contacts = contacts.ToArray();
        }

        /// <summary>
        /// Returns true if the collision has any contacts, false otherwise.
        /// </summary>
        public static implicit operator bool (PlatformCollision collision)
        {
            return collision._contacts != null && collision._contacts.Length > 0;
        }

        /// <summary>
        /// A single instance of platform collision, of which many may make up PlatformCollision.
        /// </summary>
        public struct Contact
        {
            /// <summary>
            /// Indicates the sensor that registered the contact.
            /// </summary>
            public readonly SensorType Sensor;

            /// <summary>
            /// Raycast hit and geometry data.
            /// </summary>
            public readonly TerrainCastHit HitData;

            /// <summary>
            /// The controller that came into contact with the platform.
            /// </summary>
            public HedgehogController Controller { get { return HitData ? HitData.Controller : null; } }

            /// <summary>
            /// The platform trigger that came into contact with the controller, if any.
            /// </summary>
            public readonly PlatformTrigger PlatformTrigger;

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

            public Contact(SensorType sensor, TerrainCastHit hitData, PlatformTrigger platformTrigger = null)
            {
                PlatformTrigger = platformTrigger;
                Sensor = sensor;
                HitData = hitData;

                Velocity = hitData.Controller.Velocity;
                RelativeVelocity = hitData.Controller.RelativeVelocity;
                GroundVelocity = hitData.Controller.GroundVelocity;
                SurfaceAngle = hitData.Controller.SurfaceAngle;
                RelativeSurfaceAngle = hitData.Controller.RelativeSurfaceAngle;
            }

            /// <summary>
            /// Returns true if the contact has any data, false otherwise.
            /// </summary>
            public static implicit operator bool (Contact contact)
            {
                return contact.HitData;
            }
        }
    }
}
