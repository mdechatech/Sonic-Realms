using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.UI
{
    public class FadeToBlackTransition : SceneTransition
    {
        public Material Material;

        public float FadeInTime;
        public float FadeOutTime;

        protected float FadeTimer;

        private static int _redID;
        private static int _blueID;
        private static int _greenID;

        public override void Reset()
        {
            base.Reset();

            FadeInTime = 1f;
            FadeOutTime = 1f;
        }

        public override void Awake()
        {
            base.Awake();

            FadeTimer = 0f;

            if (_redID == 0)
            {
                _redID = Shader.PropertyToID("_Red");
                _blueID = Shader.PropertyToID("_Blue");
                _greenID = Shader.PropertyToID("_Green");
            }
            

            Material.SetFloat(_redID, 0f);
            Material.SetFloat(_blueID, 0f);
            Material.SetFloat(_greenID, 0f);
        }

        public override void Start()
        {
            base.Start();
            InitCameras();
        }

        public bool HasInitCameras()
        {
            return Camera.allCameras[0].GetComponent<BlitMaterial>();
        }

        public override void OnLevelWasLoaded(int level)
        {
            base.OnLevelWasLoaded(level);
            if(!HasInitCameras()) InitCameras();
        }

        protected void InitCameras()
        {
            foreach (var camera in Camera.allCameras)
            {
                InitCamera(camera);
            }
        }

        public void InitCamera(Camera camera)
        {
            var blitMaterial = camera.gameObject.AddComponent<BlitMaterial>();
            blitMaterial.Material = Material;
        }

        protected override void NextScene()
        {
            base.NextScene();
            Time.timeScale = 1f;
        }

        protected override void OnGo()
        {
            FadeTimer = 0f;
        }

        public override void OnPlayingUpdate()
        {
            if (State == TransitionState.Enter)
            {
                FadeTimer += Time.unscaledDeltaTime;

                if (FadeTimer > FadeInTime)
                {
                    FadeTimer = 0f;

                    Material.SetFloat(_redID, -1f);
                    Material.SetFloat(_greenID, -1f);
                    Material.SetFloat(_blueID, -1f);

                    EnterComplete();
                }
                else
                {
                    var t = FadeTimer/FadeInTime*3f;
                    Material.SetFloat(_redID, -Mathf.Clamp01(t));
                    Material.SetFloat(_greenID, -Mathf.Clamp01(t - 1f));
                    Material.SetFloat(_blueID, -Mathf.Clamp01(t - 2f));
                }
            }
            else if(State == TransitionState.Exit)
            {
                FadeTimer += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                if (FadeTimer > FadeOutTime)
                {
                    FadeTimer = 0f;

                    Material.SetFloat(_redID, 0f);
                    Material.SetFloat(_blueID, 0f);
                    Material.SetFloat(_greenID, 0f);

                    ExitComplete();
                }
                else
                {
                    var t = (1f - FadeTimer/FadeOutTime)*3f;
                    Material.SetFloat(_redID, -Mathf.Clamp01(t));
                    Material.SetFloat(_greenID, -Mathf.Clamp01(t - 1f));
                    Material.SetFloat(_blueID, -Mathf.Clamp01(t - 2f));
                }
            }
        }
    }
}
