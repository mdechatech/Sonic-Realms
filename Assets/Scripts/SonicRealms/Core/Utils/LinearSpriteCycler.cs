using System.Collections;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Sets the renderer's sprite by going down a list of sprites over time. A much faster
    /// alternative to using an Animator.
    /// </summary>
    public class LinearSpriteCycler : SpriteCycler
    {
        /// <summary>
        /// If greater than zero, the object is destroyed after this many animation cycles.
        /// </summary>
        [Tooltip("If greater than zero, the object is destroyed after this many animation cycles.")]
        public int DestroyAfterCycles;

        /// <summary>
        /// If true, the cycler will go back to the first sprite after reaching the last.
        /// </summary>
        [Tooltip("If true, the cycler will go back to the first sprite after reaching the last.")]
        public bool Loop;

        /// <summary>
        /// How long before flipping to the next sprite, in seconds.
        /// </summary>
        [Tooltip("How long before flipping to the next sprite, in seconds.")]
        public float TimeBetweenSprites;

        [Foldout("Debug")]
        public bool IsPlaying;

        public float CurrentTime
        {
            get { return (float)CurrentIndex/Sprites.Length; }
            set
            {
                CurrentIndex = (int)((value*Sprites.Length)%Sprites.Length);
                SetSprite(CurrentIndex);
            }
        }

        [Foldout("Debug")]
        public int DestroyCycleCountdown;

        protected Coroutine Cycler;

        public override void Reset()
        {
            base.Reset();
            Loop = true;
            TimeBetweenSprites = 1f;
        }

        public override void Awake()
        {
            base.Awake();
            DestroyCycleCountdown = DestroyAfterCycles;
        }

        public override void Play()
        {
            if (Cycler != null) StopCoroutine(Cycler);
            Cycler = StartCoroutine(CycleSprite());
            IsPlaying = true;
        }

        public override void Pause()
        {
            if (Cycler != null) StopCoroutine(Cycler);
            IsPlaying = false;
        }

        protected IEnumerator CycleSprite()
        {
            SetSprite(CurrentIndex);

            while (true)
            {
                yield return new WaitForSeconds(TimeBetweenSprites);
                NextSprite();

                if (CurrentIndex == 0 && DestroyCycleCountdown > 0 && --DestroyCycleCountdown == 0)
                {
                    Destroy(gameObject);
                    yield break;
                }

                if (!Loop)
                {
                    if (DestroyCycleCountdown <= 0 && CurrentIndex == SpriteCount - 1 && Cycler != null)
                        StopCoroutine(Cycler);
                }
            }
        }

        public override void Stop()
        {
            Pause();
            SetSprite(0);
        }
    }
}
