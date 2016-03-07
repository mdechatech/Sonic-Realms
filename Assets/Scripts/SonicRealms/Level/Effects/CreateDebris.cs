using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using UnityEngine;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// Cuts up the sprite into a grid of game objects. You can specify your own game object for the debris
    /// or even tell it to use bits of its own sprite, like in the classic games.
    /// </summary>
    public class CreateDebris : ReactiveObject
    {
        /// <summary>
        /// Hard limit on debris created, because we don't want to crash.
        /// </summary>
        public const int DebrisLimit = 100;

        /// <summary>
        /// Copies of this object will be made for each sliced sprite, allowing movement scripts to be added.
        /// </summary>
        [Tooltip("Copies of this object will be made for each sliced sprite, allowing movement scripts to be added.")]
        public GameObject DebrisObject;

        /// <summary>
        /// The size of the debris, in units.
        /// </summary>
        [Tooltip("The size of the debris, in units.")]
        public Vector2 DebrisSize;

        /// <summary>
        /// Whether to use the sprite for the created debris.
        /// </summary>
        [Tooltip("Whether to use the sprite for the created debris.")]
        public bool UseSprite;

        /// <summary>
        /// The sprite renderer to break up.
        /// </summary>
        [Tooltip("The sprite renderer to break up.")]
        public SpriteRenderer SpriteRenderer;

        public override void Reset()
        {
            base.Reset();
            UseSprite = true;
            SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            DebrisSize = new Vector2(0.16f, 0.16f);
        }

        public override void OnActivate(HedgehogController controller)
        {
            Break(controller);
        }

        public void Break(HedgehogController controller)
        {
            var sprite = SpriteRenderer.sprite;
            var texture2D = sprite.texture;
            var rect = sprite.textureRect;
            var debrisSize = DebrisSize * sprite.pixelsPerUnit;

            var iterX =
                (int)Mathf.Max(0, Mathf.Min(Mathf.Sqrt(DebrisLimit), Mathf.CeilToInt(rect.width / debrisSize.x)));
            var iterY =
                (int)Mathf.Max(0, Mathf.Min(Mathf.Sqrt(DebrisLimit), Mathf.CeilToInt(rect.height / debrisSize.y)));

            var normalizedPivot = new Vector2(sprite.pivot.x / rect.size.x, sprite.pivot.y / rect.size.y);
            var offsetX = -Mathf.RoundToInt((rect.size.x - debrisSize.x) * normalizedPivot.x);
            var offsetY = -Mathf.RoundToInt((rect.size.y - debrisSize.y) * normalizedPivot.y);

            var offset = new Vector2(offsetX, offsetY) / sprite.pixelsPerUnit;

            for (var i = 0; i < iterX; ++i)
            {
                for (var j = 0; j < iterY; ++j)
                {
                    var debrisObject = DebrisObject == null ? new GameObject() : Instantiate(DebrisObject);
                    debrisObject.transform.SetParent(transform);
                    debrisObject.transform.localPosition = new Vector2(DebrisSize.x * i, DebrisSize.y * j) + offset;
                    debrisObject.transform.SetParent(null);
                    debrisObject.transform.eulerAngles = transform.eulerAngles;
                    debrisObject.transform.localScale = transform.lossyScale;

                    var debrisData = debrisObject.AddComponent<DebrisData>();
                    debrisData.Controller = controller;
                    debrisData.Source = this;
                    debrisData.Row = j;
                    debrisData.TotalRows = iterY;
                    debrisData.Column = i;
                    debrisData.TotalColumns = iterX;

                    if (!UseSprite) continue;
                    var rectOffset = new Vector2(rect.x + debrisSize.x * i, rect.y + debrisSize.y * j);

                    var debrisSprite = Sprite.Create(texture2D,
                        new Rect(rectOffset.x, rectOffset.y,
                        Mathf.Min(rect.xMax - rectOffset.x, debrisSize.x),
                        Mathf.Min(rect.yMax - rectOffset.y, debrisSize.y)),
                        normalizedPivot);

                    var debrisSpriteRenderer = debrisObject.GetComponent<SpriteRenderer>();
                    if (debrisSpriteRenderer == null)
                        debrisSpriteRenderer = debrisObject.AddComponent<SpriteRenderer>();
                    debrisSpriteRenderer.sprite = debrisSprite;
                }
            }
        }
    }
}
