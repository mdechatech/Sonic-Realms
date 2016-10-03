using UnityEngine;

namespace SonicRealms.Level
{
    public class RealmsCamera : MonoBehaviour
    {
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
        private int _baseWidth;

        private int _prevBaseWidth;

        [SerializeField]
        private int _baseHeight;

        private int _prevBaseHeight;

        [Header("Shaders")]
        [SerializeField]
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

        protected void OnValidate()
        {
            if (_prevBaseHeight != _baseHeight)
            {
                if (_baseHeight < 1)
                    _baseHeight = 1;

                _prevBaseHeight = _baseHeight;

                SetRTSize(_baseWidth, _baseHeight);
            }

            if (_prevBaseWidth != _baseWidth)
            {
                if (_baseWidth < 1)
                    _baseWidth = 1;

                _prevBaseWidth = _baseWidth;

                SetRTSize(_baseWidth, _baseHeight);
            }
        }

        private void SetRTSize(int width, int height)
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
            Shader.SetGlobalTexture(_globalBackgroundTextureName, _backgroundRenderTexture);
            Shader.SetGlobalTexture(_globalForegroundTextureName, _foregroundRenderTexture);
            Shader.SetGlobalTexture(_globalOverlayTextureName, _overlayRenderTexture);
        }
    }
}
