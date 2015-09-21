using UnityEngine;

namespace Hedgehog.Terrain
{
    [AddComponentMenu("Hedgehog/Platforms/Movers/Platform Mover")]
    public class PlatformMover : MonoBehaviour
    {
        /// <summary>
        /// The platform's speed in units per second.
        /// </summary>
        [SerializeField, Tooltip("Speed in units per second.")]
        public Vector2 Speed;

        public void FixedUpdate()
        {
            transform.position += (Vector3)(Speed * Time.fixedDeltaTime);
        }
    }
}
