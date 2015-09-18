using UnityEngine;

namespace Hedgehog.Examples
{
    public class ExamplePlatformOscillator : MonoBehaviour
    {
        [SerializeField] public Vector2 StartPoint;
        [SerializeField] public Vector2 EndPoint;

        [SerializeField] public float Duration = 10.0f;

        [SerializeField, Range(0.0f, 1.0f)] public float Smoothness = 1.0f;

        public void FixedUpdate()
        {
            transform.position = Vector2.Lerp(StartPoint, EndPoint,
                Mathf.Lerp(
                    (Time.fixedTime/Duration)%1.0f < 0.5f ?
                    (Time.fixedTime/Duration*2)%1.0f : 
                    1.0f - (Time.fixedTime/Duration*2)%1.0f,
                    Mathf.Sin((Time.fixedTime - Mathf.PI)/Duration*DMath.DoublePi)*0.5f + 0.5f,
                    Smoothness));
        }
    }
}
