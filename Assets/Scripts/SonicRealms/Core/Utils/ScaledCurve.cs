using System;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    [Serializable]
    public class ScaledCurve
    {
        public float Scale;
        public AnimationCurve Curve;

        public float Evaluate(float time)
        {
            return Scale*Curve.Evaluate(time);
        }
    }
}
