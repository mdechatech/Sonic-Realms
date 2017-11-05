using System;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public abstract class SrLegacyTimerView : MonoBehaviour
    {
        public abstract void Show(TimeSpan time);
    }
}
