using UnityEngine;

namespace SonicRealms.Level
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteRenderer))]
    public class WaterOverlay : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Material _waterMaterial;

        [SerializeField, HideInInspector]
        private Sprite _waterSprite;

        [SerializeField]
        private Texture _displacementTexture;

        [SerializeField]
        private Texture _backgroundColorCurveTexture;

        [SerializeField]
        private Color _backgroundColorTint;

        [SerializeField]
        private Texture _foregroundColorCurveTexture;

        [SerializeField]
        private Color _foregroundColorTint;

        [SerializeField]
        [Range(0, 0.1f)]
        private float _magnitude;

        [SerializeField]
        private Vector2 _speed;

        [SerializeField]
        [Range(0.5f, 4)]
        private float _displacementScale;

        protected void Reset()
        {
            var sprite = GetComponent<SpriteRenderer>();
            sprite.sharedMaterial = _waterMaterial;
            sprite.sprite = _waterSprite;
            sprite.sortingLayerName = "Overlay Effects";
            sprite.gameObject.layer = LayerMask.NameToLayer("Water");

            _magnitude = 0.02f;
            _speed = new Vector2(0.3f, 0.3f);
            _displacementScale = 1;
            _backgroundColorTint = new Color(0, 0, 1, 0);
            _foregroundColorTint = new Color(0, 0, 1, 0);
        }

#if UNITY_EDITOR
        protected void Update()
        {
            var sprite = GetComponent<SpriteRenderer>();

            if (!sprite.sharedMaterial)
                sprite.sharedMaterial = _waterMaterial;

            var block = new MaterialPropertyBlock();

            if(_displacementTexture)
                block.SetTexture("_DisplaceTex", _displacementTexture);

            if(_backgroundColorCurveTexture)
                block.SetTexture("_BGColorTex", _backgroundColorCurveTexture);

            if(_foregroundColorCurveTexture)
                block.SetTexture("_FGColorTex", _foregroundColorCurveTexture);

            block.SetFloat("_Magnitude", _magnitude);
            block.SetFloat("_HorizSpeed", _speed.x);
            block.SetFloat("_VertSpeed", _speed.y);

            block.SetFloat("_DisplaceScaleX", 1/sprite.bounds.size.x*_displacementScale);
            block.SetFloat("_DisplaceScaleY", 1/sprite.bounds.size.y*_displacementScale);

            block.SetColor("_BGColorTint", _backgroundColorTint);
            block.SetColor("_FGColorTint", _foregroundColorTint);

            sprite.SetPropertyBlock(block);
        }
#endif
    }
}
