using UnityEngine;

namespace SonicRealms.Level
{
    public class RealmsCamera : MonoBehaviour
    {
        public float OrthographicSize
        {
            get { return _orthographicSize; }
            set
            {
                _orthographicSize = value;
                UpdateOrthographicSize();
            }
        }

        public int BaseWidth
        {
            get { return _baseWidth; }
            set
            {
                _baseWidth = value;
                UpdateRenderTextureSize();
            }
        }

        public int BaseHeight
        {
            get { return _baseHeight; }
            set
            {
                _baseHeight = value;
                UpdateRenderTextureSize();
            }
        }

        public string GlobalBackgroundTextureName
        {
            get { return _globalBackgroundTextureName; }
            set
            {
                _globalBackgroundTextureName = value;
                SetGlobalTextures();
            }
        }

        public string GlobalForegroundTextureName
        {
            get { return _globalForegroundTextureName; }
            set
            {
                _globalForegroundTextureName = value;
                SetGlobalTextures();
            }
        }

        public string GlobalOverlayTextureName
        {
            get { return _globalOverlayTextureName; }
            set
            {
                _globalOverlayTextureName = value;
                SetGlobalTextures();
            }
        }

        [SerializeField, HideInInspector]
        private RenderTexture _backgroundRenderTexture;

        [SerializeField, HideInInspector]
        private RenderTexture _foregroundRenderTexture;

        [SerializeField, HideInInspector]
        private RenderTexture _overlayRenderTexture;

        [SerializeField, HideInInspector]
        private Camera _backgroundCamera;

        [SerializeField, HideInInspector]
        private Camera _foregroundCamera;

        [SerializeField, HideInInspector]
        private Camera _overlayCamera;

        [SerializeField, HideInInspector]
        private Camera _rendererCamera;

        [SerializeField, HideInInspector]
        private MeshRenderer _cameraQuad;

        [Header("Dimensions")]
        [SerializeField]
        [Tooltip("Orthographic size of all cameras used by the Realms Camera.")]
        private float _orthographicSize;

        private float _prevOrthographicSize;

        [SerializeField]
        [Tooltip("Width of the texture to which cameras render. The texture is then resized to fit the screen.")]
        private int _baseWidth;

        private int _prevBaseWidth;

        [SerializeField]
        [Tooltip("Height of the texture to which cameras render. The texture is then resized to fit the screen.")]
        private int _baseHeight;

        private int _prevBaseHeight;

        [Header("Shaders")]
        [SerializeField]
        [Tooltip("If not empty, ")]
        private string _globalBackgroundTextureName;

        [SerializeField]
        private string _globalForegroundTextureName;

        [SerializeField]
        private string _globalOverlayTextureName;
        
        [SerializeField, HideInInspector]
        private Camera _baseBackgroundCamera;

        [SerializeField, HideInInspector]
        private Camera _baseForegroundCamera;

        [SerializeField, HideInInspector]
        private Camera _baseOverlayCamera;

        [SerializeField, HideInInspector]
        private Camera _baseRendererCamera;

        [SerializeField, HideInInspector]
        private MeshRenderer _baseRendererQuad;

        [SerializeField, HideInInspector]
        private Material _baseRendererMaterial;
        
        public RenderTexture BackgroundRenderTexture { get { return _backgroundRenderTexture; } }
        public RenderTexture ForegroundRenderTexture { get { return _foregroundRenderTexture; } }

        protected void Reset()
        {
            for (var i = transform.childCount - 1; i >= 0; --i)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            _baseWidth = 320;
            _baseHeight = 224;

            _globalBackgroundTextureName = "_GlobalBackgroundTex";
            _globalForegroundTextureName = "_GlobalForegroundTex";
            _globalOverlayTextureName = "_GlobalOverlayTex";

            GenerateCameras();
            GenerateRenderTextures();
            SetGlobalTextures();
            GenerateCameraQuad();
        }

        protected void Awake()
        {
            SetGlobalTextures();
        }

#if UNITY_EDITOR
        protected void Update()
        {
            if (!Application.isPlaying)
                SetGlobalTextures();
        }
#endif

        protected void OnValidate()
        {
            if (_prevOrthographicSize != _orthographicSize)
            {
                UpdateOrthographicSize();

                _prevOrthographicSize = _orthographicSize;
            }

            if (_prevBaseHeight != _baseHeight)
            {
                if (_baseHeight < 1)
                    _baseHeight = 1;

                SetRenderTextureSize(_baseWidth, _baseHeight);

                _prevBaseHeight = _baseHeight;
            }

            if (_prevBaseWidth != _baseWidth)
            {
                if (_baseWidth < 1)
                    _baseWidth = 1;

                SetRenderTextureSize(_baseWidth, _baseHeight);

                _prevBaseWidth = _baseWidth;
            }
        }

        private void UpdateOrthographicSize()
        {
            SetOrthographicSize(_orthographicSize);
        }

        private void SetOrthographicSize(float orthographicSize)
        {
            _backgroundCamera.orthographicSize = orthographicSize;
            _foregroundCamera.orthographicSize = orthographicSize;
            _overlayCamera.orthographicSize = orthographicSize;
        }

        private void UpdateRenderTextureSize()
        {
            SetRenderTextureSize(_baseWidth, _baseHeight);
        }

        private void SetRenderTextureSize(int width, int height)
        {
            if (_backgroundRenderTexture != null)
            {
                _backgroundRenderTexture.Release();
            }

            if (_foregroundRenderTexture != null)
            {
                _foregroundRenderTexture.Release();
            }

            if (_overlayRenderTexture != null)
            {
                _overlayRenderTexture.Release();
            }

            _backgroundRenderTexture = _backgroundCamera.targetTexture = new RenderTexture(width, height, 16);
            _backgroundRenderTexture.filterMode = FilterMode.Point;

            _foregroundRenderTexture = _foregroundCamera.targetTexture = new RenderTexture(width, height, 16);
            _foregroundRenderTexture.filterMode = FilterMode.Point;

            _overlayRenderTexture = _overlayCamera.targetTexture = new RenderTexture(width, height, 16);
            _overlayRenderTexture.filterMode = FilterMode.Point;

            _cameraQuad.transform.localScale = new Vector3(_baseWidth / (float)_baseHeight,
                _cameraQuad.transform.localScale.y,
                _cameraQuad.transform.localScale.z);

            SetGlobalTextures();
        }

        private void GenerateCameras()
        {
            if (_backgroundCamera)
            {
                DestroyImmediate(_backgroundCamera.gameObject);
            }

            if (_foregroundCamera)
            {
                DestroyImmediate(_foregroundCamera.gameObject);
            }

            if (_overlayCamera)
            {
                DestroyImmediate(_overlayCamera.gameObject);
            }

            if (_rendererCamera)
            {
                DestroyImmediate(_rendererCamera.gameObject);
            }

            _backgroundCamera = Instantiate(_baseBackgroundCamera);
            _backgroundCamera.transform.SetParent(transform);
            _backgroundCamera.transform.localPosition = Vector3.zero;
            _backgroundCamera.name = "Background Camera";

            _foregroundCamera = Instantiate(_baseForegroundCamera);
            _foregroundCamera.transform.SetParent(transform);
            _foregroundCamera.transform.localPosition = Vector3.zero;
            _foregroundCamera.name = "Foreground Camera";

            _overlayCamera = Instantiate(_baseOverlayCamera);
            _overlayCamera.transform.SetParent(transform);
            _overlayCamera.transform.localPosition = Vector3.zero;
            _overlayCamera.name = "Overlay Camera";

            _rendererCamera = Instantiate(_baseRendererCamera);
            _rendererCamera.transform.SetParent(transform);
            _rendererCamera.transform.localPosition = new Vector3(1000, 1000);
            _rendererCamera.name = "Renderer Camera";
        }

        private void GenerateRenderTextures()
        {
            if (_backgroundRenderTexture != null)
            {
                DestroyImmediate(_backgroundRenderTexture);
                _backgroundRenderTexture = null;
            }

            if (_foregroundRenderTexture != null)
            {
                DestroyImmediate(_foregroundRenderTexture);
                _foregroundRenderTexture = null;
            }

            if (_overlayRenderTexture != null)
            {
                DestroyImmediate(_overlayRenderTexture);
                _overlayRenderTexture = null;
            }

            _backgroundCamera.targetTexture = _backgroundRenderTexture = new RenderTexture(_baseWidth, _baseHeight, 16);
            _backgroundRenderTexture.filterMode = FilterMode.Point;

            _foregroundCamera.targetTexture = _foregroundRenderTexture = new RenderTexture(_baseWidth, _baseHeight, 16);
            _foregroundRenderTexture.filterMode = FilterMode.Point;

            _overlayCamera.targetTexture = _overlayRenderTexture = new RenderTexture(_baseWidth, _baseHeight, 16);
            _overlayRenderTexture.filterMode = FilterMode.Point;
        }

        private void GenerateCameraQuad()
        {
            if (_cameraQuad)
            {
                DestroyImmediate(_cameraQuad.gameObject);
                _cameraQuad = null;
            }

            _cameraQuad = Instantiate(_baseRendererQuad);
            _cameraQuad.transform.SetParent(_rendererCamera.transform);
            _cameraQuad.transform.localPosition = new Vector3(0, 0, 10);
            _cameraQuad.sharedMaterial = _baseRendererMaterial;
            _cameraQuad.name = "Renderer Quad";

            _cameraQuad.transform.localScale = new Vector3(_baseWidth/(float) _baseHeight,
                _cameraQuad.transform.localScale.y,
                _cameraQuad.transform.localScale.z);
        }

        private void SetGlobalTextures()
        {
            if (!string.IsNullOrEmpty(_globalBackgroundTextureName))
                Shader.SetGlobalTexture(_globalBackgroundTextureName, _backgroundRenderTexture);

            if (!string.IsNullOrEmpty(_globalForegroundTextureName))
                Shader.SetGlobalTexture(_globalForegroundTextureName, _foregroundRenderTexture);

            if (!string.IsNullOrEmpty(_globalOverlayTextureName))
                Shader.SetGlobalTexture(_globalOverlayTextureName, _overlayRenderTexture);
        }
    }
}
