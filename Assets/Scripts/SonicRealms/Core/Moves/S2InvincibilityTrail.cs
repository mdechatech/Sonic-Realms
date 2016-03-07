using System.Collections;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class S2InvincibilityTrail : MonoBehaviour
    {
        public GameObject[] Sparkles;

        [Space]
        public float Frequency;
        public int Amount;
        public float Life;

        public void Reset()
        {
            Sparkles = new GameObject[0];
            Frequency = 20f;
            Amount = 2;
            Life = 0.2f;
        }

        public void Start()
        {
            StartCoroutine(Spawn());
        }

        protected IEnumerator Spawn()
        {
            while (true)
            {
                for (var i = 0; i < Amount; ++i)
                {
                    var sparkle = Instantiate(Sparkles[DMath.Modp(i, Sparkles.Length)]);
                    sparkle.transform.position = transform.position;
                    Destroy(sparkle, Life);
                }

                yield return new WaitForSeconds(1f/Frequency);
            }
        }
    }
}
