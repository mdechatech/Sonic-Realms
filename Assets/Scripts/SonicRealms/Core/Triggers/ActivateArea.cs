using SonicRealms.Core.Actors;

namespace SonicRealms.Core.Triggers
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

        public override void OnAreaEnter(Hitbox hitbox)
        {
            ActivateObject(hitbox.Controller);
        }

        public override void OnAreaExit(Hitbox hitbox)
        {
            DeactivateObject(hitbox.Controller);
        }
    }
}
