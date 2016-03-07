using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Temporary fix to particle bug in 5.3.1f1
    /// </summary>
    [ExecuteInEditMode]
    public class DisableInactiveParticles : MonoBehaviour
    {
        ParticleSystem.Particle[] unused = new ParticleSystem.Particle[1];

        void Awake()
        {
            GetComponent<ParticleSystemRenderer>().enabled = false;
        }

        void LateUpdate()
        {
            GetComponent<ParticleSystemRenderer>().enabled = GetComponent<ParticleSystem>().GetParticles(unused) > 0;
        }
    }
}
