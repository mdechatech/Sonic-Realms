using System;
using UnityEngine;

namespace SonicRealms.UI
{
    [Serializable]
    public struct TransitionWaitModule : IEquatable<TransitionWaitModule>
    {
        /// <summary>
        /// If greater than zero, the trigger will occur after this many seconds.
        /// </summary>
        [Tooltip("If greater than zero, the trigger will occur after this many seconds.")]
        public float Time;

        /// <summary>
        /// The trigger will occur after this animation is no longer playing.
        /// </summary>
        [Tooltip("The trigger will occur after this animation is no longer playing.")]
        public string Animation;

        /// <summary>
        /// The trigger will occur after this transition is no longer playing.
        /// </summary>
        [Tooltip("The trigger will occur after this transition is no longer playing.")]
        public Transition Transition;

        /// <summary>
        /// The transition will play this sound and wait for it to end before proceeding.
        /// </summary>
        [Tooltip("The transition will play this sound and wait for it to end before proceeding.")]
        public AudioClip Sound;

        public bool Equals(TransitionWaitModule other)
        {
            return Time.Equals(other.Time) && string.Equals(Animation, other.Animation) &&
                   Equals(Transition, other.Transition) && Equals(Sound, other.Sound);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TransitionWaitModule && Equals((TransitionWaitModule) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Time.GetHashCode();
                hashCode = (hashCode*397) ^ (Animation != null ? Animation.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Transition != null ? Transition.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Sound != null ? Sound.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TransitionWaitModule left, TransitionWaitModule right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TransitionWaitModule left, TransitionWaitModule right)
        {
            return !left.Equals(right);
        }
    }
}
