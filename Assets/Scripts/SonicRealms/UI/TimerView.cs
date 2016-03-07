using System;
using UnityEngine;

namespace SonicRealms.UI
{
    public abstract class TimerView : MonoBehaviour
    {
        public abstract void Show(TimeSpan time);
    }
}
