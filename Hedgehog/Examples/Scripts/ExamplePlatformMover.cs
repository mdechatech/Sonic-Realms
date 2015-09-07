using UnityEngine;

namespace Hedgehog.Examples
{
    public class ExamplePlatformMover : MonoBehaviour
    {
        public float dx;
        public float dy;

        public void FixedUpdate()
        {
            transform.position += new Vector3(dx, dy) * Time.fixedDeltaTime;
        }
    }
}
