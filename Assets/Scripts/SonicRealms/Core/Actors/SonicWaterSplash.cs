using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using SonicRealms.Level.Areas;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Handles water splash effects for Sonic.
    /// </summary>
    public class SonicWaterSplash : MonoBehaviour
    {
        public HedgehogController Player;

        public LayerMask WaterLayer;

        public GameObject Splash;

        /// <summary>
        /// Invoked when the player leaves a body of water.
        /// </summary>
        public UnityEvent OnEntrySplash;

        /// <summary>
        /// Invoked when the player enters a body of water.
        /// </summary>
        public UnityEvent OnExitSplash;

        public void Reset()
        {
            Player = GetComponentInParent<HedgehogController>();
            WaterLayer = 1 << LayerMask.NameToLayer("Water");
        }

        public void Awake()
        {
            OnEntrySplash = OnEntrySplash ?? new UnityEvent();
            OnExitSplash = OnExitSplash ?? new UnityEvent();
        }

        public void Start()
        {
            Player.OnAreaEnter.AddListener(OnAreaEnter);
            Player.OnAreaExit.AddListener(OnAreaExit);
        }

        protected void OnAreaEnter(ReactiveArea area)
        {
            if (!(area is Water) || !Player.Inside<Water>()) return;

            var entry = FindWaterEntry();
            if (!entry) return;

            var splash = Instantiate(Splash);
            splash.transform.position = entry.point;
            splash.transform.eulerAngles = new Vector3(
                splash.transform.eulerAngles.x,
                splash.transform.eulerAngles.y,
                DMath.Angle(entry.normal)*Mathf.Rad2Deg - 90f);

            OnEntrySplash.Invoke();
        }

        protected void OnAreaExit(ReactiveArea area)
        {
            if (!(area is Water) || Player.Inside<Water>()) return;

            var exit = FindWaterExit();
            if (!exit) return;

            var splash = Instantiate(Splash);
            splash.transform.position = exit.point;
            splash.transform.eulerAngles = new Vector3(
                splash.transform.eulerAngles.x,
                splash.transform.eulerAngles.y,
                DMath.Angle(exit.normal)*Mathf.Rad2Deg - 90f);

            OnExitSplash.Invoke();
        }

        // Q: Why is there so much code just to find the water line?
        // A: It is designed to work with surfaces of water in any orientation, in any direction of gravity.
        // Try it out with slanted water, for example!

        public RaycastHit2D FindWaterEntry()
        {
            var hit = Physics2D.Linecast((Vector2)Player.transform.position - Player.Velocity,
                    Player.transform.position,
                    WaterLayer);

            if (!hit || hit.fraction == 0f)
            {
                hit = Physics2D.Linecast((Vector2) Player.transform.position -
                                         DMath.AngleToVector(Player.GravityDirection*Mathf.Rad2Deg),
                    Player.transform.position, WaterLayer);
            }

            if (hit.fraction == 0f)
                return default(RaycastHit2D);

            return hit;
        }

        public RaycastHit2D FindWaterExit()
        {
            var hit = Physics2D.Linecast(Player.transform.position,
                (Vector2) Player.transform.position - Player.Velocity,
                WaterLayer);

            if (!hit || hit.fraction == 0f)
            {
                hit = Physics2D.Linecast(Player.transform.position,
                    (Vector2) Player.transform.position +
                    DMath.AngleToVector(Player.GravityDirection*Mathf.Rad2Deg)
                    , WaterLayer);
            }

            if (hit.fraction == 0f)
                return default(RaycastHit2D);

            return hit;
        }
    }
}
