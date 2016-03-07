using SonicRealms.Core.Triggers;
using SonicRealms.Level.Platforms;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Events for when the player runs on water.
    /// </summary>
    public class WaterRunningTrigger : MonoBehaviour
    {
        public HedgehogController Player;

        /// <summary>
        /// Invoked when the player starts running on water.
        /// </summary>
        public UnityEvent OnStartRunning;

        /// <summary>
        /// Invoked when the player stops running on water.
        /// </summary>
        public UnityEvent OnStopRunning;

        private bool _wasRunning;

        public void Reset()
        {
            Player = GetComponentInParent<HedgehogController>();
        }

        public void Awake()
        {
            OnStartRunning = OnStartRunning ?? new UnityEvent();
            OnStopRunning = OnStopRunning ?? new UnityEvent();
        }

        public void Start()
        {
            Player.OnPlatformSurfaceEnter.AddListener(OnSurface);
            Player.OnPlatformSurfaceExit.AddListener(OnSurface);
        }

        protected void OnSurface(ReactivePlatform platform)
        {
            var running = Player.StandingOn<WaterSurface>();
            if (running && !_wasRunning)
            {
                _wasRunning = true;
                OnStartRunning.Invoke();
            }
            else if (!running && _wasRunning)
            {
                _wasRunning = false;
                OnStopRunning.Invoke();
            }
        }
        
    }
}
