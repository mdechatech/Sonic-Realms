using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Sets the renderer's sprite based on an animation curve over time. A much faster
    /// alternative to the Animator.
    /// </summary>
    public class CurveSpriteCycler : SpriteCycler
    {
        /// <summary>
        /// If greater than zero, the object is destroyed after this many animation cycles.
        /// </summary>
        [Tooltip("If greater than zero, the object is destroyed after this many animation cycles.")]
        public int DestroyAfterCycles;

        /// <summary>
        /// If true, the cycler's time will go back to 0 after reaching 1.
        /// </summary>
        [Tooltip("If true, the cycler's time will go back to 0 after reaching 1.")]
        public bool Loop;

        /// <summary>
        /// Maps current time (0 - 1) to the x axis and sprite index (0 - 1) to the y axis.
        /// </summary>
        [Tooltip("Maps current time (0 - 1) to the x axis and sprite index (0 - 1) to the y axis.")]
        public AnimationCurve Curve;

        /// <summary>
        /// Time it takes to get through the curve.
        /// </summary>
        [Tooltip("Time it takes to get through the curve.")]
        public float CycleLength;

        [Foldout("Debug")]
        public bool IsPlaying;

        [Foldout("Debug")]
        public int DestroyCycleCountdown;

        [Foldout("Debug")]
        private float _currentTime;
        public float CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                SetSprite(_currentTime);
            }
        }

        public override void Reset()
        {
            base.Reset();
            DestroyCycleCountdown = DestroyAfterCycles;
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            CycleLength = 1f;
        }

        public override void Play()
        {
            IsPlaying = true;
        }

        public override void Pause()
        {
            IsPlaying = false;
        }

        public void Update()
        {
            if (!IsPlaying) return;

            if ((_currentTime += Time.deltaTime) > 1f)
            {
                if (DestroyCycleCountdown > 0 && --DestroyCycleCountdown == 0)
                {
                    Destroy(gameObject);
                    return;
                }

                if (!Loop)
                {
                    _currentTime = 1f;
                    Pause();
                }
                else
                    _currentTime %= 1f;
            }

            SetSprite(_currentTime);
        }


        public override void SetSprite(float time)
        {
            SetSprite((int)(Curve.Evaluate(time)*SpriteCount));
        }

        public override void Stop()
        {
            Pause();
            SetSprite(0);
        }
    }
}
