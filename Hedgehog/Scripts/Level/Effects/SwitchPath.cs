using Hedgehog.Core.Actors;
using Hedgehog.Core.Triggers;
using UnityEngine;

namespace Hedgehog.Level.Effects
{
    /// <summary>
    /// When activated by a controller, switches its path. Great for springs or buttons that put you on
    /// new paths.
    /// </summary>
    [AddComponentMenu("Hedgehog/Object Effects/Switch Path")]
    public class SwitchPath : ReactiveObject
    {
        /// <summary>
        /// Whether the controller must be on the ground to activate.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the controller must be on the ground to activate.")]
        public bool MustBeGrounded;

        /// <summary>
        /// Whether to do the opposite (if on ToPath, switch to FromPath) if going backwards on the ground.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to do the opposite (if on ToPath, switch to FromPath) if going backwards on the ground.")]
        public bool UndoIfGoingBackwards;

        /// <summary>
        /// The name of the path the controller must be on to activate.
        /// </summary>
        [SerializeField]
        [Tooltip("The name of the path the controller must be on to activate.")]
        public string FromPath;

        /// <summary>
        /// The name of the path the controller is put onto.
        /// </summary>
        [SerializeField]
        [Tooltip("The name of the path the controller is put onto.")]
        public string ToPath;

        public override void Reset()
        {
            base.Reset();
            MustBeGrounded = true;
            UndoIfGoingBackwards = true;
            FromPath = "Path 1";
            ToPath = "Path 2";
        }

        public override void OnActivateEnter(HedgehogController controller)
        {
            if (MustBeGrounded)
            {
                if (!controller.Grounded) return;

                if (UndoIfGoingBackwards)
                {
                    if (controller.GroundVelocity >= 0.0f && controller.Paths.Contains(FromPath))
                    {
                        controller.Paths.Remove(FromPath);
                        controller.Paths.Add(ToPath);
                    }
                    else if(controller.Paths.Contains(ToPath))
                    {
                        controller.Paths.Remove(ToPath);
                        controller.Paths.Add(FromPath);
                    }
                }
                else if(controller.Paths.Contains(FromPath))
                {
                    controller.Paths.Remove(FromPath);
                    controller.Paths.Add(ToPath);
                }
            }
            else if(controller.Paths.Contains(FromPath))
            {
                controller.Paths.Remove(FromPath);
                controller.Paths.Add(ToPath);
            }
        }
    }
}
