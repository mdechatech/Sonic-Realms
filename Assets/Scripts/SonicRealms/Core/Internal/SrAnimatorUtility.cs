using System;
using UnityEngine;

namespace SonicRealms.Core.Internal
{
    public static class SrAnimatorUtility
    {
        public static void SilentSet(Component component, Action<Animator> setter)
        {
            SilentSet(component.gameObject, setter);
        }

        public static void SilentSet(GameObject gameObject, Action<Animator> setter)
        {
            var animator = gameObject.GetComponent<Animator>();
            if (animator)
                SilentSet(animator, setter);
        }

        public static void SilentSet(Animator animator, Action<Animator> setter)
        {
            if (setter == null)
                return;

            var logWarnings = animator.logWarnings;
            animator.logWarnings = false;

            setter(animator);

            animator.logWarnings = logWarnings;
        }

        public static void SilentSet<T>(Component component, T value, Action<Animator, T> setter)
        {
            SilentSet(component.gameObject, value, setter);
        }

        public static void SilentSet<T>(GameObject gameObject, T value, Action<Animator, T> setter)
        {
            var animator = gameObject.GetComponent<Animator>();
            if (animator)
                SilentSet(animator, value, setter);
        }

        public static void SilentSet<T>(Animator animator, T value, Action<Animator, T> setter)
        {
            if (setter == null)
                return;

            var logWarnings = animator.logWarnings;
            animator.logWarnings = false;

            setter(animator, value);

            animator.logWarnings = logWarnings;
        }
    }
}
