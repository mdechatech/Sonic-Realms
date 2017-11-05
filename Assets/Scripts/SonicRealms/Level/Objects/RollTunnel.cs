using System;
using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Moves;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Objects
{
    /// <summary>
    /// A tunnel that can be rolled through, like the s-tunnel in Green Hill Zone.
    /// </summary>
    public class RollTunnel : MonoBehaviour
    {
        public enum DirectionMode
        {
            Forward,
            Backward,
            BothWays
        }

        public enum PassengerDirection
        {
            Forward,
            Backward
        }

        public List<AreaTrigger> BackwardEntrances { get { return _backwardEntrances; } set { _backwardEntrances = value; } }

        public List<AreaTrigger> ForwardEntrances { get { return _forwardEntrances; } set { _forwardEntrances = value; } }

        public List<AreaTrigger> ForwardExits { get { return _forwardExits; } set { _forwardExits = value; } }

        public List<AreaTrigger> BackwardExits { get { return _backwardExits; } set { _backwardExits = value; } }

        public DirectionMode PushDirection { get { return _pushDirection; } set { _pushDirection = value; } }

        public List<Transform> Ceilings { get { return _ceilings; } set { _ceilings = value; } }

        public float PushAmount { get { return _pushAmount; } set { _pushAmount = value; } }

        public float Acceleration { get { return _acceleration; } set { _acceleration = value; } }

        [SerializeField, SrFoldout("Triggers")]
        [Tooltip("Where the player would enter the tunnel if it were moving forward.")]
        private List<AreaTrigger> _forwardEntrances;

        [SerializeField, SrFoldout("Triggers")]
        [Tooltip("Where the player would enter the tunnel if it were moving backward.")]
        private List<AreaTrigger> _backwardEntrances;
        
        [SerializeField, SrFoldout("Triggers")]
        [Tooltip("Where the player would exit the tunnel if it were moving forward.")]
        private List<AreaTrigger> _forwardExits;

        [SerializeField, SrFoldout("Triggers")]
        [Tooltip("Where the player would exit the tunnel if it were moving backward.")]
        private List<AreaTrigger> _backwardExits;
        

        [SerializeField, SrFoldout("Physics")]
        [Tooltip("For the ceilings of the tunnel - pushes the player in the opposite direction to account for " +
                 "the player running upside down.")]
        private List<Transform> _ceilings;
            
        [SerializeField, SrFoldout("Physics")]
        [Tooltip("Direction in which the tunnel pushes the player.")]
        private DirectionMode _pushDirection;

        [SerializeField, SrFoldout("Physics")]
        [Tooltip("Amount to push the player by when it comes to a standstill in units per second.")]
        private float _pushAmount;

        [SerializeField, SrFoldout("Physics")]
        [Tooltip("Constant acceleration on the player in units per seconds squared.")]
        private float _acceleration;

        private List<PassengerInfo> _passengers;

        public PassengerInfo GetPassengerInfo(HedgehogController player)
        {
            return _passengers.FirstOrDefault(passenger => passenger.Player == player);
        }

        public bool IsOnCeiling(HedgehogController player)
        {
            return _ceilings.Any(ceiling => player.IsStandingOn(ceiling));
        }

        public bool IsPassenger(HedgehogController player)
        {
            return _passengers.Any(passenger => passenger.Player == player);
        }

        protected virtual void Reset()
        {
            _pushAmount = 1;
        }

        protected virtual void Awake()
        {
            _passengers = new List<PassengerInfo>();

            _forwardEntrances = _forwardEntrances ?? new List<AreaTrigger>();
            _backwardEntrances = _backwardEntrances ?? new List<AreaTrigger>();
            _forwardExits = _forwardExits ?? new List<AreaTrigger>();
            _backwardExits = _backwardExits ?? new List<AreaTrigger>();

            _ceilings = _ceilings ?? new List<Transform>();
        }

        protected virtual void Start()
        {
            foreach (var entrance in _forwardEntrances)
            {
                entrance.OnAreaEnter.AddListener(OnEnterForward);
                entrance.OnAreaStay.AddListener(OnEnterForward);

                entrance.OnAreaExit.AddListener(FinishEnteringForward);
            }

            foreach (var entrance in _backwardEntrances)
            {
                entrance.OnAreaEnter.AddListener(OnEnterBackward);
                entrance.OnAreaStay.AddListener(OnEnterBackward);

                entrance.OnAreaExit.AddListener(FinishEnteringBackward);
            }

            foreach (var exit in _forwardExits)
            {
                exit.OnAreaEnter.AddListener(OnExitForward);
                exit.OnAreaStay.AddListener(OnExitForward);
            }

            foreach (var exit in _backwardExits)
            {
                exit.OnAreaEnter.AddListener(OnExitBackward);
                exit.OnAreaStay.AddListener(OnExitBackward);
            }
        }

        protected virtual void FixedUpdate()
        {
            foreach (var passenger in _passengers)
            {
                var stopped = (passenger.Direction == PassengerDirection.Forward &&
                               passenger.Player.GroundVelocity > -0.1f && passenger.Player.GroundVelocity <= 0) ||
                              (passenger.Direction == PassengerDirection.Backward &&
                               passenger.Player.GroundVelocity < 0.1f && passenger.Player.GroundVelocity >= 0);

                bool isOnCeiling = IsOnCeiling(passenger.Player);

                bool pushForward;

                switch (_pushDirection)
                {
                    default:
                    case DirectionMode.Forward:
                        pushForward = true;
                        break;

                    case DirectionMode.Backward:
                        pushForward = false;
                        break;

                    case DirectionMode.BothWays:
                        pushForward = (passenger.Direction == PassengerDirection.Forward);
                        break;
                }

                if (isOnCeiling)
                    pushForward = !pushForward;

                if (pushForward)
                {
                    if (stopped)
                        passenger.Player.GroundVelocity += _pushAmount;

                    passenger.Player.GroundVelocity += _acceleration*Time.fixedDeltaTime;
                }
                else
                {
                    if (stopped)
                        passenger.Player.GroundVelocity -= _pushAmount;

                    passenger.Player.GroundVelocity -= _acceleration*Time.fixedDeltaTime;
                }
            }
        }

        private void OnEnterForward(AreaCollision collision)
        {
            var player = collision.Controller;

            if (!IsPassenger(player))
                AddPassenger(player, PassengerDirection.Forward);
        }

        private void OnExitForward(AreaCollision collision)
        {
            var player = collision.Controller;
            if (!IsPassenger(player))
                return;

            var passenger = GetPassengerInfo(player);

            if (!passenger.IsEntering)
                RemovePassenger(player);
        }

        private void OnEnterBackward(AreaCollision collision)
        {
            var player = collision.Controller;

            if (!IsPassenger(player))
                AddPassenger(player, PassengerDirection.Backward);
        }

        private void OnExitBackward(AreaCollision collision)
        {
            var player = collision.Controller;
            if (!IsPassenger(player))
                return;

            var passenger = GetPassengerInfo(player);

            if (!passenger.IsEntering)
                RemovePassenger(player);
        }

        private void FinishEnteringForward(AreaCollision collision)
        {
            var player = collision.Controller;
            if (!IsPassenger(player))
                return;

            var passenger = GetPassengerInfo(player);

            if (passenger.Direction == PassengerDirection.Forward)
                passenger.IsEntering = false;
        }

        private void FinishEnteringBackward(AreaCollision collision)
        {
            var player = collision.Controller;
            if (!IsPassenger(player))
                return;

            var passenger = GetPassengerInfo(player);

            if (passenger.Direction == PassengerDirection.Backward)
                passenger.IsEntering = false;
        }

        private void AddPassenger(HedgehogController player, PassengerDirection direction)
        {
            var roll = player.GetMove<Roll>();
            if (!roll)
                return;

            roll.EndOnStop = false;
            roll.EndOnLanding = false;

            //player.DisableWallDetach = true;
            player.Perform<Roll>(true);

            _passengers.Add(new PassengerInfo(player, direction));
        }

        private void RemovePassenger(HedgehogController player)
        {
            var playerIndex = _passengers.FindIndex(passenger => passenger.Player == player);
            if (playerIndex < 0)
                return;

            var roll = player.GetMove<Roll>();
            if (!roll)
                return;

            roll.EndOnStop = true;
            roll.EndOnLanding = true;
            //player.DisableWallDetach = false;

            _passengers.RemoveAt(playerIndex);
        }

        public class PassengerInfo
        {
            public readonly HedgehogController Player;
            public readonly PassengerDirection Direction;
            public bool IsEntering;

            public PassengerInfo(HedgehogController player, PassengerDirection direction)
            {
                Player = player;
                Direction = direction;
                IsEntering = true;
            }
        }
    }
}
