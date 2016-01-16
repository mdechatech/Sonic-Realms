using System;
using UnityEngine;

namespace Hedgehog.UI
{
    public abstract class BaseTimerDisplay : MonoBehaviour
    {
        public abstract void Display(TimeSpan time);
    }
}
