using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Generic area that activates the object trigger when a controller is inside it.
    /// </summary>
    public class ActivateArea : ReactiveArea
    {
        public override void Reset()
        {
            base.Reset();
            if (!GetComponent<ObjectTrigger>()) gameObject.AddComponent<ObjectTrigger>();
        }

        public override void OnAreaEnter(HedgehogController controller)
        {
            ActivateObject(controller);
        }

        public override void OnAreaExit(HedgehogController controller)
        {
            DeactivateObject(controller);
        }
    }
}
