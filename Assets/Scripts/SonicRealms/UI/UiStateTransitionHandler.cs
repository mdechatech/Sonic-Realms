using System.Collections;
using UnityEngine;

namespace SonicRealms.UI
{
    public abstract class UiStateTransitionHandler : MonoBehaviour
    {
        public abstract IEnumerator Handle(string fromState, string toState);

        public abstract void Skip(string fromState, string toState);
    }
}
