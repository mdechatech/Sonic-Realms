using System.Collections;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public abstract class SrLegacyUiStateTransitionHandler : MonoBehaviour
    {
        public abstract IEnumerator Handle(string fromState, string toState);

        public abstract void Skip(string fromState, string toState);
    }
}
