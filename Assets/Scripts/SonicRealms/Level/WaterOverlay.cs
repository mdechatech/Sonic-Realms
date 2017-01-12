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
        private bool _updateMaterial;

        [Space]
        [SerializeField]
        private Texture _displacementTexture;

        [Space]
        [SerializeField]
        private Texture _backgroundColorCurveTexture;

        [SerializeField]
        private Color _backgroundColorTint;

        [Space]
        [SerializeField]
        private Texture _foregroundColorCurveTexture;

        [SerializeField]
        private Color _foregroundColorTint;

        [Space]
        [SerializeField]
        [Range(0, 0.1f)]
        private float _magnitude;

        [SerializeField]
        private Vector2 _speed;

        [SerializeField]
        [Range(0.5f, 4)]
        private float _displacementScale;

        private SpriteRenderer _spriteRenderer;

        private static bool _hasInitializedIds;

        private static int _displaceTexId;
        private static int _bgColorTexId;
        private static int _fgColorTexId;
        private static int _magnitudeId;
        private static int _horizSpeedId;
        private static int _vertSpeedId;
        private static int _displaceScaleXId;
        private static int _displaceScaleYId;
        private static int _bgColorTintId;
        private static int _fgColorTintId;

        private MaterialPropertyBlock _block;

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

        protected void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (!_hasInitializedIds)
                InitializePropertyIds();

            UpdateMaterialProperties();
        }

#if UNITY_EDITOR
        protected void Start()
        {
            if (!Application.isPlaying)
                Awake();
        }
#endif
        protected void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UpdateMaterialProperties();
                return;
            }
#endif
            if (_updateMaterial)
            {
                UpdateMaterialProperties();
            }
        }

        private static void InitializePropertyIds()
        {
            _hasInitializedIds = true;

            _displaceTexId = Shader.PropertyToID("_DisplaceTex");
            _bgColorTexId = Shader.PropertyToID("_BGColorTex");
            _fgColorTexId = Shader.PropertyToID("_FGColorTex");
            _magnitudeId = Shader.PropertyToID("_Magnitude");
            _horizSpeedId = Shader.PropertyToID("_HorizSpeed");
            _vertSpeedId = Shader.PropertyToID("_VertSpeed");
            _displaceScaleXId = Shader.PropertyToID("_DisplaceScaleX");
            _displaceScaleYId = Shader.PropertyToID("_DisplaceScaleY");
            _bgColorTintId = Shader.PropertyToID("_BGColorTint");
            _fgColorTintId = Shader.PropertyToID("_FGColorTint");
        }

        public void UpdateMaterialProperties()
        {
            if (!_spriteRenderer.sharedMaterial)
                _spriteRenderer.sharedMaterial = _waterMaterial;

            if (_block == null)
                _block = new MaterialPropertyBlock();

            if (_displacementTexture)
                _block.SetTexture(_displaceTexId, _displacementTexture);

            if (_backgroundColorCurveTexture)
                _block.SetTexture(_bgColorTexId, _backgroundColorCurveTexture);

            if (_foregroundColorCurveTexture)
                _block.SetTexture(_fgColorTexId, _foregroundColorCurveTexture);

            _block.SetFloat(_magnitudeId, _magnitude);
            _block.SetFloat(_horizSpeedId, _speed.x);
            _block.SetFloat(_vertSpeedId, _speed.y);

            _block.SetFloat(_displaceScaleXId, 1 / _spriteRenderer.bounds.size.x * _displacementScale);
            _block.SetFloat(_displaceScaleYId, 1 / _spriteRenderer.bounds.size.y * _displacementScale);

            _block.SetColor(_bgColorTintId, _backgroundColorTint);
            _block.SetColor(_fgColorTintId, _foregroundColorTint);

            _spriteRenderer.SetPropertyBlock(_block);
        }
    }
}
