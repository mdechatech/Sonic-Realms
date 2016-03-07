using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Platforms
{
    /// <summary>
    /// Records horizontal or vertical position of a controller when on the platform.
    /// </summary>
    public class PlatformProgressAnimator : ReactivePlatform
    {
        /// <summary>
        /// Animator float on the controller set to the controller's progress on the platform.
        /// </summary>
        [Tooltip("Animator float on the controller set to the controller's progress on the platform.")]
        public string ProgressFloat;

        /// <summary>
        /// Whether to use the horizontal or vertical axis.
        /// </summary>
        [Tooltip("Whether to use the horizontal or vertical axis.")]
        public bool Horizontal;

        /// <summary>
        /// Whether to have the start of the platform be the right horizontally/the top vertically.
        /// </summary>
        [Tooltip("Whether to have the start of the platform be the right horizontally/the top vertically.")]
        public bool ReverseAxis;

        /// <summary>
        /// Progress is set to this at the beginning of the platform.
        /// </summary>
        [Tooltip("Progress is set to this at the beginning of the platform.")]
        public float ProgressMin;

        /// <summary>
        /// Progress is set to this at the end of the platform.
        /// </summary>
        [Tooltip("Progress is set to this at the end of the platform.")]
        public float ProgressMax;

        /// <summary>
        /// The total bounds of all colliders under this object.
        /// </summary>
        protected Bounds Bounds;

        public override void Reset()
        {
            base.Reset();
            ProgressFloat = "Corkscrew Progress";
            Horizontal = true;
            ReverseAxis = false;

            ProgressMin = 0.0f;
            ProgressMax = 1.0f;
        }

        public override void Start()
        {
            base.Start();

            // Get the total width and height of the platform by summing up all its colliders
            foreach (var collider2D in GetComponentsInChildren<Collider2D>())
            {
                if (Bounds == default(Bounds))
                    Bounds.SetMinMax(collider2D.bounds.min, collider2D.bounds.max);
                else
                    Bounds.Encapsulate(collider2D.bounds);
            }
        }

        public override void OnSurfaceStay(TerrainCastHit hit)
        {
            if (hit.Controller == null) return;

            var progress = GetProgress(hit.Controller.transform.position);

            // Set progress on the controller's animator
            if (hit.Controller.Animator == null) return;
            var logWarnings = hit.Controller.Animator.logWarnings;
            hit.Controller.Animator.logWarnings = false;
            hit.Controller.Animator.SetFloat(ProgressFloat, progress);
            hit.Controller.Animator.logWarnings = logWarnings;
        }

        /// <summary>
        /// Returns the progress of an object with the specified position.
        /// </summary>
        /// <param name="position">The specified position.</param>
        /// <returns></returns>
        public virtual float GetProgress(Vector3 position)
        {
            return 
                // Player position between the bounds as a number between 0 and 1...
                ((Horizontal
                    ? Mathf.Clamp01((position.x - Bounds.min.x)/Bounds.size.x)
                    : Mathf.Clamp01((position.y - Bounds.min.y)/Bounds.size.y))

                // Flipped if ReverseAxis is true...
                * (ReverseAxis ? -1.0f : 1.0f) + (ReverseAxis ? 1.0f : 0.0f))

                // And proportionalized between ProgressMin and ProgressMax
                * (ProgressMax - ProgressMin) + ProgressMin;
        }
    }
}
