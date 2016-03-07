using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level
{
    /// <summary>
    /// Manipulates the CameraController based on the state of the target player.
    /// </summary>
    [RequireComponent(typeof(CameraController))]
    public class SonicCameraController : MonoBehaviour
    {
        public static Dictionary<HedgehogController, List<SonicCameraController>> Cameras;

        public CameraController Camera;
        public HedgehogController Player;
        protected HealthSystem Health;

        [Space]
        public bool RotateToGravity;

        [Range(0f, 1f)]
        public float RotateSmoothness;

        #region Follow Ground State
        /// <summary>
        /// How quickly the camera follows the player on the ground, in units per second.
        /// </summary>
        [Foldout("Follow Ground")]
        [Tooltip("How quickly the camera follows the player on the ground, in units per second.")]
        public Vector2 GroundFollowSpeed;

        /// <summary>
        /// The lower-left corner of the camera's follow boundaries when on the ground.
        /// </summary>
        [Space, Foldout("Follow Ground")]
        [Tooltip("The lower-left corner of the camera's follow boundaries when on the ground.")]
        public Transform GroundFollowBoundsMin;

        /// <summary>
        /// The upper-right corner of the camera's follow boundaries when on the ground.
        /// </summary>
        [Foldout("Follow Ground")]
        [Tooltip("The upper-right corner of the camera's follow boundaries when on the ground.")]
        public Transform GroundFollowBoundsMax;

        /// <summary>
        /// How quickly the player must be moving to begin fast following.
        /// </summary>
        [Space, Foldout("Follow Ground")]
        [Tooltip("How quickly the player must be moving to begin fast following.")]
        public Vector2 GroundFastFollowMinSpeed;

        /// <summary>
        /// How quickly the camera fast follows the player on the ground, in units per second.
        /// </summary>
        [Foldout("Follow Ground")]
        [Tooltip("How quickly the camera fast follows the player on the ground, in units per second.")]
        public Vector2 GroundFastFollowSpeed;
        #endregion
        #region Follow Air State
        /// <summary>
        /// How quickly the camera follows the player in the air, in units per second.
        /// </summary>
        [Foldout("Follow Air")]
        [Tooltip("How quickly the camera follows the player in the air, in units per second.")]
        public Vector2 AirFollowSpeed;

        /// <summary>
        /// The lower-left corner of the camera's follow boundaries when in the air.
        /// </summary>
        [Space, Foldout("Follow Air")]
        [Tooltip("The lower-left corner of the camera's follow boundaries when in the air.")]
        public Transform AirFollowBoundsMin;

        /// <summary>
        /// The upper-right corner of the camera's follow boundaries when in the air.
        /// </summary>
        [Foldout("Follow Air")]
        [Tooltip("The upper-right corner of the camera's follow boundaries when in the air.")]
        public Transform AirFollowBoundsMax;
        #endregion
        #region LookUp State
        /// <summary>
        /// Current amount the camera has scrolled, in units.
        /// </summary>
        protected float LookAmount;

        /// <summary>
        /// Whether the camera is returning to its neutral position after looking.
        /// </summary>
        protected bool LookReturning;

        /// <summary>
        /// When looking up, how long to wait before panning upwards.
        /// </summary>
        [Foldout("Look Up")]
        [Tooltip("When looking up, how long to wait before panning upwards.")]
        public float LookUpLag;

        /// <summary>
        /// When looking up, how far up to scroll, in units.
        /// </summary>
        [Foldout("Look Up")]
        [Tooltip("When looking up, how far up to scroll, in units.")]
        public float LookUpPanAmount;

        /// <summary>
        /// When looking up, how quickly the camera scrolls, in units per second.
        /// </summary>
        [Foldout("Look Up")]
        [Tooltip("When looking up, how quickly the camera scrolls, in units per second.")]
        public float LookUpPanSpeed;
        #endregion
        #region LookDown State
        /// <summary>
        /// When looking down, how long to wait before panning downwards.
        /// </summary>
        [Foldout("Look Down")]
        [Tooltip("When looking down, how long to wait before panning downwards.")]
        public float LookDownLag;

        /// <summary>
        /// When looking down, how far down to scroll, in units.
        /// </summary>
        [Foldout("Look Down")]
        [Tooltip("When looking down, how far down to scroll, in units.")]
        public float LookDownPanAmount;

        /// <summary>
        /// When looking down, how quickly the camera scrolls, in units per second.
        /// </summary>
        [Foldout("Look Down")]
        [Tooltip("When looking down, how quickly the camera scrolls, in units per second.")]
        public float LookDownPanSpeed;
        #endregion
        #region Spindash
        /// <summary>
        /// Time left in the lag state.
        /// </summary>
        [HideInInspector]
        public float LagTimer;

        /// <summary>
        /// How long to lag after a spindash, in seconds.
        /// </summary>
        [Foldout("Spindash")]
        [Tooltip("How long to lag after a spindash, in seconds.")]
        public float SpindashLag;
        #endregion
        #region ForwardShift State
        /// <summary>
        /// If true, spindash lag will not occur.
        /// Whether to shift the camera forward when moving at a certain speed, as seen in Sonic CD.
        /// </summary>
        [Foldout("Forward Shift (Sonic CD)")]
        [Tooltip("If checked, spindash lag will not occur. " +
                 "Whether to shift the camera forward when moving at a certain speed, as seen in Sonic CD.")]
        public bool EnableForwardShift;

        /// <summary>
        /// Minimum horizontal speed for the forward shift to occur.
        /// </summary>
        [Space, Foldout("Forward Shift (Sonic CD)")]
        [Tooltip("Minimum horizontal speed for the forward shift to occur.")]
        public float ForwardShiftMinSpeed;

        /// <summary>
        /// How far the camera shifts forward, in units.
        /// </summary>
        [Foldout("Forward Shift (Sonic CD)")]
        [Tooltip("How far the camera shifts forward, in units.")]
        public float ForwardShiftPanAmount;

        /// <summary>
        /// How quickly the camera shifts forward, in units per second.
        /// </summary>
        [Foldout("Forward Shift (Sonic CD)")]
        [Tooltip("How quickly the camera shifts forward, in units per second.")]
        public float ForwardShiftPanSpeed;
        #endregion

        protected MoveManager MoveManager;

        private bool _looking;
        private bool _forwardShifting;
        private bool _fastFollowing;
        private bool _finished;

        static SonicCameraController()
        {
            Cameras = new Dictionary<HedgehogController, List<SonicCameraController>>();
        }

        public virtual void Reset()
        {
            GroundFollowSpeed = new Vector2(9.6f, 3.6f);
            GroundFastFollowMinSpeed = new Vector2(0f, 3.6f);
            GroundFastFollowSpeed = new Vector2(9.6f, 9.6f);

            AirFollowSpeed = new Vector2(9.6f, 9.6f);

            LookUpLag = 2.0f;
            LookUpPanAmount = 1.04f;
            LookUpPanSpeed = 1.2f;

            LookDownLag = 2.0f;
            LookDownPanAmount = 0.88f;
            LookDownPanSpeed = 1.2f;

            SpindashLag = 0.266667f;

            EnableForwardShift = false;
            ForwardShiftMinSpeed = 3.59f;
            ForwardShiftPanAmount = 0.64f;
            ForwardShiftPanSpeed = 1.2f;
        }

        public virtual void Awake()
        {
            _looking = false;
            _forwardShifting = false;
            _fastFollowing = false;
        }

        public virtual void Start()
        {
            Camera = GetComponent<CameraController>();
            Camera.OnChangeTarget.AddListener(OnChangeTarget);
            if (Player) Camera.Target = Player.transform;
        }

        protected virtual void OnChangeTarget(Transform previousTarget)
        {
            if (Player != null)
            {
                if (MoveManager != null)
                {
                    MoveManager.OnPerform.RemoveListener(OnPerformMove);
                    MoveManager.OnEnd.RemoveListener(OnEndMove);
                }

                if (Health != null)
                {
                    Health.OnDeath.RemoveListener(OnDeath);
                }

                List<SonicCameraController> previousCameras;
                if (Cameras.TryGetValue(Player, out previousCameras))
                    previousCameras.Remove(this);
            }

            Player = Camera.Target == null ? null : Camera.Target.GetComponent<HedgehogController>();
            if (Player == null) return;

            Player.OnAttach.AddListener(OnAttach);
            Player.OnDetach.AddListener(OnDetach);

            MoveManager = Player.GetComponent<MoveManager>();
            if (MoveManager)
            {
                MoveManager.OnPerform.AddListener(OnPerformMove);
                MoveManager.OnEnd.AddListener(OnEndMove);
            }

            Health = Player.GetComponent<HealthSystem>();
            if (Health != null)
            {
                Health.OnDeath.AddListener(OnDeath);
            }

            List<SonicCameraController> cameras;
            if (!Cameras.TryGetValue(Player, out cameras))
                Cameras.Add(Player, cameras = new List<SonicCameraController>());
            cameras.Add(this);

            Camera.Snap();
        }

        public void FinishLevel(Transform focusTarget)
        {
            FinishLevel(focusTarget, Camera.FocusSpeed);
        }

        public void FinishLevel(Transform focusTarget, Vector2 focusSpeed)
        {
            _finished = true;
            Camera.Rotate(0f);
            Camera.Focus(focusTarget, focusSpeed);
        }

        /// <summary>
        /// Pauses the camera, as if after the player performed a spindash.
        /// </summary>
        public void DoSpindashLag()
        {
            Camera.Wait(SpindashLag);
        }

        /// <summary>
        /// Shifts the camera forward or backward based on the player's horizontal speed, like in Sonic CD.
        /// </summary>
        public void DoForwardShift()
        {
            DoForwardShift(Player.RelativeVelocity.x >= 0f);
        }

        /// <summary>
        /// Shifts the camera forward or backward, like in Sonic CD.
        /// </summary>
        /// <param name="forward">Whether to shift the camera forward or backward.</param>
        public void DoForwardShift(bool forward)
        {
            _forwardShifting = true;
            Camera.PanAtSpeed(
                DMath.AngleToVector(Player.RelativeAngle(forward ? 0 : 180)*Mathf.Deg2Rad)*ForwardShiftPanAmount,
                ForwardShiftPanSpeed);
        }

        public virtual void FixedUpdate()
        {
            if (_finished || Player == null) return;

            if (RotateToGravity)
            {
                if (DMath.Equalsf(RotateSmoothness))
                {
                    Camera.Rotate(Player.GravityDirection + 90.0f);
                }
                else
                {
                    Camera.Rotate(Mathf.LerpAngle(Camera.Rotation, Player.GravityDirection + 90.0f,
                        Time.fixedDeltaTime * (1.0f / RotateSmoothness)));
                }
            }
            else
            {
                Camera.Rotate(0.0f);
            }

            if (Player != null && Player.Grounded)
            {
                var abs = new Vector2(Mathf.Abs(Player.Vx), Mathf.Abs(Player.Vy));
                if (_fastFollowing)
                {
                    if (!(abs.x > GroundFastFollowMinSpeed.x && abs.y > GroundFastFollowMinSpeed.y))
                    {
                        _fastFollowing = false;
                        Camera.FollowSpeed = GroundFollowSpeed;
                    }
                }
                else
                {
                    if (abs.x > GroundFastFollowMinSpeed.x && abs.y > GroundFastFollowMinSpeed.y)
                    {
                        _fastFollowing = true;
                        Camera.FollowSpeed = GroundFastFollowSpeed;
                    }
                }
            }

            if (EnableForwardShift)
            {
                if (Mathf.Abs(Player.RelativeVelocity.x) > ForwardShiftMinSpeed)
                {
                    DoForwardShift();
                }
                else if (!_looking && _forwardShifting)
                {
                    _forwardShifting = false;
                    Camera.Pan(Vector2.zero, ForwardShiftPanSpeed);
                }
            }
        }

        public void OnAttach()
        {
            Camera.FollowSpeed = GroundFollowSpeed;
            Camera.FollowBoundsMin = GroundFollowBoundsMin;
            Camera.FollowBoundsMax = GroundFollowBoundsMax;
        }

        public void OnDetach()
        {
            Camera.FollowSpeed = AirFollowSpeed;
            Camera.FollowBoundsMin = AirFollowBoundsMin;
            Camera.FollowBoundsMax = AirFollowBoundsMax;
        }

        public void OnPerformMove(Move move)
        {
            if (move is FlameSpecial && !EnableForwardShift)
            {
                DoSpindashLag();
            }
            else if (move is LookUp)
            {
                _looking = true;
                Camera.PanAtSpeed(DMath.AngleToVector(Player.RelativeAngle(90f) * Mathf.Deg2Rad) * LookUpPanAmount,
                    LookUpPanSpeed,
                    LookUpLag);
            }
            else if (move is Duck)
            {
                _looking = true;
                Camera.PanAtSpeed(DMath.AngleToVector(Player.RelativeAngle(-90f) * Mathf.Deg2Rad) * LookDownPanAmount,
                    LookDownPanSpeed, LookDownLag);
            }
            else if (move is Spindash)
            {
                if (!EnableForwardShift)
                    Camera.Pan(Vector2.zero, Camera.PanTime);
                else
                    DoForwardShift(Player.FacingForward);
            }
            else if (move is Roll)
            {
                var dy = Vector2.up*-((Roll)move).HeightChange*0.5f;
                Camera.ExtraOffset = dy;
                Camera.BasePosition -= dy;
            }
        }

        public void OnEndMove(Move move)
        {
            if (move is Spindash)
            {
                if (!EnableForwardShift)
                {
                    DoSpindashLag();
                }
            }
            else if (move is LookUp)
            {
                _looking = false;
                Camera.PanAtSpeed(Vector2.zero, LookUpPanSpeed);
            }
            else if (move is Duck)
            {
                _looking = false;
                Camera.PanAtSpeed(Vector2.zero, LookDownPanSpeed);
            }
            else if (move is Roll)
            {
                var dy = Vector2.up * -((Roll)move).HeightChange * 0.5f;
                Camera.ExtraOffset = Vector2.zero;
                Camera.BasePosition += dy;
            }
        }

        protected void OnDeath(HealthEventArgs e)
        {
            Camera.Wait(999f);
        }
    }

    public static class SonicCameraControllerExtensions
    {
        public static SonicCameraController GetCamera(this HedgehogController player)
        {
            return SonicCameraController.Cameras[player].FirstOrDefault();
        }

        public static List<SonicCameraController> GetCameras(this HedgehogController player)
        {
            return SonicCameraController.Cameras[player];
        } 
    }
}
