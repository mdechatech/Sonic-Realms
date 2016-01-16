using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Moves;
using Hedgehog.Core.Utils;
using UnityEngine;

namespace Hedgehog.Level
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

        #region Follow State
        /// <summary>
        /// Maximum speed at which the camera follows the controller, in units per second.
        /// </summary>
        [Header("Follow")]
        [Tooltip("Maximum speed at which the camera follows the controller, in units per second.")]
        public float FollowSpeed;

        /// <summary>
        /// Center of the view border, in units. Center of the camera is (0, 0).
        /// </summary>
        [Tooltip("Center of the view border, in units. Center of the camera is (0, 0).")]
        public Vector2 FollowCenter;

        /// <summary>
        /// Half the size of the view border, in units. If the player is this much away from the center, the camera
        /// starts catching up.
        /// </summary>
        [Tooltip("Half the size of the view border, in units. If the player is this much away from the center, the camera " +
                 "starts catching up.")]
        public Vector2 FollowRadius;
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
        [Header("Look Up")]
        [Tooltip("When looking up, how long to wait before panning upwards.")]
        public float LookUpLag;

        /// <summary>
        /// When looking up, how far up to scroll, in units.
        /// </summary>
        [Tooltip("When looking up, how far up to scroll, in units.")]
        public float LookUpPanAmount;

        /// <summary>
        /// When looking up, how quickly the camera scrolls, in units per second.
        /// </summary>
        [Tooltip("When looking up, how quickly the camera scrolls, in units per second.")]
        public float LookUpPanSpeed;
        #endregion
        #region LookDown State
        /// <summary>
        /// When looking down, how long to wait before panning downwards.
        /// </summary>
        [Header("Look Down")]
        [Tooltip("When looking down, how long to wait before panning downwards.")]
        public float LookDownLag;

        /// <summary>
        /// When looking down, how far down to scroll, in units.
        /// </summary>
        [Tooltip("When looking down, how far down to scroll, in units.")]
        public float LookDownPanAmount;

        /// <summary>
        /// When looking down, how quickly the camera scrolls, in units per second.
        /// </summary>
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
        [Header("Spindash")]
        [Tooltip("How long to lag after a spindash, in seconds.")]
        public float SpindashLag;
        #endregion
        #region ForwardShift State
        /// <summary>
        /// Whether to shift the camera forward when moving at a certain speed, as seen in Sonic CD.
        /// </summary>
        [Header("Forward Shift (Sonic CD)")]
        [Tooltip("Whether to shift the camera forward when moving at a certain speed, as seen in Sonic CD.")]
        public bool EnableForwardShift;

        /// <summary>
        /// Minimum horizontal speed for the forward shift to occur.
        /// </summary>
        [Tooltip("Minimum horizontal speed for the forward shift to occur.")]
        public float ForwardShiftMinSpeed;

        /// <summary>
        /// How far the camera shifts forward, in units.
        /// </summary>
        [Tooltip("How far the camera shifts forward, in units.")]
        public float ForwardShiftPanAmount;

        /// <summary>
        /// How quickly the camera shifts forward, in units per second.
        /// </summary>
        [Tooltip("How quickly the camera shifts forward, in units per second.")]
        public float ForwardShiftPanSpeed;
        #endregion

        private bool _looking;
        private bool _forwardShifting;

        static SonicCameraController()
        {
            Cameras = new Dictionary<HedgehogController, List<SonicCameraController>>();
        }

        public virtual void Reset()
        {
            FollowSpeed = 9.6f;
            FollowCenter = new Vector2(0.0f, 0.0f);
            FollowRadius = new Vector2(0.08f, 0.32f);

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
        }

        public virtual void Start()
        {
            Camera = GetComponent<CameraController>();
            Camera.OnChangeTarget.AddListener(OnChangeTarget);
            Camera.Target = Player.transform;
        }

        protected virtual void OnChangeTarget(Transform previousTarget)
        {
            if (Player != null)
            {
                Player.MoveManager.OnPerform.RemoveListener(OnPerformMove);
                Player.MoveManager.OnEnd.RemoveListener(OnEndMove);

                List<SonicCameraController> previousCameras;
                if (Cameras.TryGetValue(Player, out previousCameras))
                    previousCameras.Remove(this);
            }

            Player = Camera.Target == null ? null : Camera.Target.GetComponent<HedgehogController>();
            if (Player == null) return;

            Player.MoveManager.OnPerform.AddListener(OnPerformMove);
            Player.MoveManager.OnEnd.AddListener(OnEndMove);

            List<SonicCameraController> cameras;
            if (!Cameras.TryGetValue(Player, out cameras))
                Cameras.Add(Player, cameras = new List<SonicCameraController>());
            cameras.Add(this);
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

        public void OnPerformMove(Move move)
        {
            if (move is FlameSpecial && !EnableForwardShift)
            {
                DoSpindashLag();
            }
            else if (move is LookUp)
            {
                _looking = true;
                Camera.PanAtSpeed(DMath.AngleToVector(Player.RelativeAngle(90f)*Mathf.Deg2Rad)*LookUpPanAmount,
                    LookUpPanSpeed,
                    LookUpLag);
            }
            else if (move is Duck)
            {
                _looking = true;
                Camera.PanAtSpeed(DMath.AngleToVector(Player.RelativeAngle(-90f)*Mathf.Deg2Rad)*LookDownPanAmount,
                    LookDownPanSpeed, LookDownLag);
            }
            else if (move is Spindash)
            {
                if (!EnableForwardShift)
                    Camera.Pan(Vector2.zero, Camera.PanTime);
                else
                    DoForwardShift(Player.FacingForward);
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
