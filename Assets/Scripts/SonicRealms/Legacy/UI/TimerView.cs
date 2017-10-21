using System;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public abstract class TimerView : MonoBehaviour
    {
        public abstract void Show(TimeSpan time);
    }
}
