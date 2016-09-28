using SonicRealms.Core.Triggers;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    public class PlatformPreCollisionEvent : UnityEvent<PlatformCollision.Contact>
    {
    }
}
