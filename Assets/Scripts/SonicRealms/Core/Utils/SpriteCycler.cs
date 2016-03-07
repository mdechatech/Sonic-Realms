using UnityEngine;

namespace SonicRealms.Core.Utils
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class SpriteCycler : MonoBehaviour
    {
        [HideInInspector]
        public SpriteRenderer SpriteRenderer;

        /// <summary>
        /// List of sprites to cycle through.
        /// </summary>
        [Tooltip("List of sprites to cycle through.")]
        public Sprite[] Sprites;
        public virtual int SpriteCount
        {
            get { return Sprites.Length; }
        }

        /// <summary>
        /// Whether to start the cycler at the start of the game.
        /// </summary>
        [Tooltip("Whether to start the cycler at the start of the game.")]
        public bool PlayOnAwake;

        /// <summary>
        /// Current sprite index in the list.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("Current sprite index in the list.")]
        public int CurrentIndex;

        public virtual void Reset()
        {
            PlayOnAwake = true;
        }

        public virtual void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            Sprites = Sprites ?? new Sprite[0];

            if (PlayOnAwake) Play();
        }

        public virtual void OnEnable()
        {
            if (SpriteRenderer == null) SpriteRenderer = GetComponent<SpriteRenderer>();
            if(PlayOnAwake) Play();
        }

        public virtual void OnDisable()
        {
            Pause();
        }

        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();

        public virtual void SetSprite(int index)
        {
            SpriteRenderer.sprite = Sprites[CurrentIndex = DMath.Modp(index, SpriteCount)];
        }

        public virtual void SetSprite(float time)
        {
            SetSprite(Mathf.FloorToInt(time*Sprites.Length));
        }

        public virtual void NextSprite()
        {
            SetSprite(CurrentIndex + 1);
        }

        public virtual void PreviousSprite()
        {
            SetSprite(CurrentIndex - 1);
        }
    }
}
