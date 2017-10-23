using System;
using UnityEngine;

namespace SonicRealms.Core.Internal
{
    [Serializable]
    public class SrScaledCurve
    {
        public float Scale;
        public AnimationCurve Curve;

        public float Evaluate(float time)
        {
            return Scale*Curve.Evaluate(time);
        }
    }
}
