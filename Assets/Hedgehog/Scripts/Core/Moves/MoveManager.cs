using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Moves
{
    #region Event Classes 
    /// <summary>
    /// Used for when a move is performed.
    /// </summary>
    [Serializable]
    public class MoveEvent : UnityEvent<Move> { }
    #endregion

    /// <summary>
    /// Handles a controller's moves.
    /// </summary>
    public class MoveManager : MonoBehaviour
    {
        /// <summary>
        /// The controller that has the moves.
        /// </summary>
        [Tooltip("The controller that has these moves.")]
        public HedgehogController Controller;

        /// <summary>
        /// List of moves.
        /// </summary>
        [Tooltip("List of moves.")]
        public List<Move> Moves;

        // Used for list copying
        private List<Move> _moves;

        #region Events
        /// <summary>
        /// Invoked when a move is performed.
        /// </summary>
        public MoveEvent OnPerform;

        /// <summary>
        /// Invoked when a move tries to be performed while the controller is interrupted.
        /// </summary>
        public MoveEvent OnInterrupted;

        /// <summary>
        /// Invoked when a move is ended.
        /// </summary>
        public MoveEvent OnEnd;

        /// <summary>
        /// Invoked when a move becomes available.
        /// </summary>
        public MoveEvent OnAvailable;

        /// <summary>
        /// Invoked when a move becomes unavailable.
        /// </summary>
        public MoveEvent OnUnavailable;

        /// <summary>
        /// Invoked when a move is added to the list.
        /// </summary>
        public MoveEvent OnAdd;

        /// <summary>
        /// Invoked when a move is removed from the list.
        /// </summary>
        public MoveEvent OnRemove;
        #endregion

        public void Reset()
        {
            Controller = GetComponentInParent<HedgehogController>();
            GetComponentsInChildren(Moves);
        }

        public void Awake()
        {
            OnPerform = OnPerform ?? new MoveEvent();
            OnInterrupted = OnInterrupted ?? new MoveEvent();
            OnEnd = OnEnd ?? new MoveEvent();
            OnAvailable = OnAvailable ?? new MoveEvent();
            OnUnavailable = OnUnavailable ?? new MoveEvent();
            OnAdd = OnAdd ?? new MoveEvent();
            OnRemove = OnRemove ?? new MoveEvent();
            Moves = GetComponentsInChildren<Move>().ToList();
            _moves = new List<Move>();
        }

        public void Start()
        {
            foreach (var move in Moves)
            {
                AddMove(move);
            }

            UpdateList();
        }

        public void Update()
        {
            if (Controller.Interrupted)
                return;

            for (var i = Moves.Count - 1; i >= 0; i = --i >= Moves.Count ? Moves.Count - 1 : i)
            {
                var move = Moves[i];

                if (!move)
                {
                    UpdateList();
                    continue;
                }

                if (!move.enabled) continue;

                if (move.CurrentState == Move.State.Unavailable)
                {
                    if (!move.Available) continue;

                    move.ChangeState(Move.State.Available);
                    OnAvailable.Invoke(move);
                } else if (move.CurrentState == Move.State.Available)
                {
                    if (!move.Available)
                    {
                        move.ChangeState(Move.State.Unavailable);
                        OnUnavailable.Invoke(move);
                    }
                    else if (move.AllowShouldPerform && move.ShouldPerform)
                    {
                        Perform(move, false, false);
                    }
                } else if (move.CurrentState == Move.State.Active)
                {
                    if (move.AllowShouldEnd && move.ShouldEnd)
                    {
                        End(move);
                        OnAvailable.Invoke(move);
                    }
                    else
                    {
                        move.OnActiveUpdate();
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            if (Controller.Interrupted) return;

            for (var i = 0; i < Moves.Count; ++i)
            {
                var move = Moves[i];

                // Might as well check to see if the move has been destroyed - if it has, update the list
                if (!move)
                {
                    UpdateList();
                    continue;
                }

                if(move.CurrentState == Move.State.Active) move.OnActiveFixedUpdate();
            }
        }

        #region Move Helpers
        /// <summary>
        /// Updates the move list. This is done automatically if it finds a move has been removed or
        /// destroyed, but not when added.
        /// </summary>
        public void UpdateList()
        {
            // Remove all destroyed/transferred moves and notify them
            for(var i = Moves.Count - 1; i >= 0; i = i-- >= Moves.Count ? Moves.Count - 1 : i)
            {
                if (i >= Moves.Count) i = Moves.Count - 1;
                var move = Moves[i];

                if (move && move.transform.IsChildOf(transform))
                    continue;

                Moves.Remove(move);

                move.OnManagerRemove();
                OnRemove.Invoke(move);
                move.OnRemove.Invoke();
            }

            // Copy the current list before filling it with new moves
            _moves.Clear();
            for (var i = Moves.Count - 1; i >= 0; i = --i >= Moves.Count ? Moves.Count - 1 : i)
                _moves.Add(Moves[i]);

            GetComponentsInChildren(Moves);

            // Compare the copied list with the new, add/notify the ones they don't have in common
            for (var i = Moves.Count - 1; i >= 0; i = --i >= Moves.Count ? Moves.Count - 1 : i)
            {
                var move = Moves[i];
                if(!_moves.Contains(move)) AddMove(move);
            }
        }

        /// <summary>
        /// Adds the specified move to the move list.
        /// </summary>
        /// <param name="move"></param>
        protected void AddMove(Move move)
        {
            move.Controller = Controller;
            move.Manager = this;

            // Pretend the move is available to see if it wants to be performed on initialization
            move.CurrentState = Move.State.Available;

            move.OnManagerAdd();
            OnAdd.Invoke(move);
            move.OnAdd.Invoke();

            // If the move didn't perform itself immediately, continue as normal
            if (!move) return;

            if (move.Active) return;

            if (move.ShouldPerform)
            {
                Perform(move);
            }
            else if (!move.Available)
            {
                move.CurrentState = Move.State.Unavailable;
            }
        }

        /// <summary>
        /// Returns the first move of the specified type.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public TMove GetMove<TMove>() where TMove : Move
        {
            return Moves.FirstOrDefault(move => move is TMove) as TMove;
        }

        /// <summary>
        /// Returns the first move in the specified group.
        /// </summary>
        /// <param name="moveGroup">The specified group.</param>
        /// <returns></returns>
        public Move GetMove(MoveGroup moveGroup)
        {
            return moveGroup == MoveGroup.All
                ? Moves.FirstOrDefault()
                : Moves.FirstOrDefault(move => move.Groups.Contains(moveGroup));
        }

        /// <summary>
        /// Returns all moves of the specified type.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public IEnumerable<TMove> GetMoves<TMove>() where TMove : Move
        {
            return Moves.OfType<TMove>();
        }

        /// <summary>
        /// Returns all moves of the specified group.
        /// </summary>
        /// <param name="moveGroup">The specified group.</param>
        /// <returns></returns>
        public IEnumerable<Move> GetMoves(MoveGroup moveGroup)
        {
            return Moves.Where(move => move.Groups.Contains(moveGroup));
        }

        /// <summary>
        /// Returns whether the first move of the specified type is available.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsAvailable<TMove>() where TMove : Move
        {
            return Moves.Any(move => move.CurrentState == Move.State.Available && move is TMove);
        }

        /// <summary>
        /// Returns whether the first move of the specified type is active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsActive<TMove>() where TMove : Move
        {
            for(var i = 0; i < Moves.Count; ++i)
            {
                var move = Moves[i];
                if (move.CurrentState != Move.State.Active) continue;
                if (move is TMove) return true;
            }

            return false;
        }

        /// <summary>
        /// Performs the first move of the specified type, if it's available.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <param name="force">Whether to perform the move even if it's unavailable.</param>
        /// <param name="mute">Whether to mute sounds that would usually play from the move.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool Perform<TMove>(bool force = false, bool mute = false) where TMove : Move
        {
            var foundMove = Moves.FirstOrDefault(move => move.CurrentState == Move.State.Available && move is TMove);

            if (force && foundMove == null)
                foundMove = Moves.FirstOrDefault(move => move.CurrentState == Move.State.Unavailable && move is TMove);

            if (foundMove == null)
                return false;

            return Perform(foundMove, force, mute);
        }

        /// <summary>
        /// Performs the first move of the specified name (invocable from the UI).
        /// </summary>
        /// <param name="moveType"></param>
        public void Perform(string moveType)
        {
            Perform(Moves.FirstOrDefault(move => move.GetType().Name == moveType));
        }

        public void Perform(Move move)
        {
            Perform(move, false);
        }

        /// <summary>
        /// Performs the specified move, if it's available.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <param name="force">Whether to perform the move even if it's unavailable.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool Perform(Move move, bool force)
        {
            return Perform(move, force, false);
        }

        /// <summary>
        /// Performs the specified move, if it's available.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <param name="force">Whether to perform the move even if it's unavailable.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool Perform(Move move, bool force, bool mute)
        {
            if (move == null || move.CurrentState == Move.State.Active)
                return false;

            if (Controller.Interrupted)
            {
                OnInterrupted.Invoke(move);
                return false;
            }

            if (force)
            {
                if (move.CurrentState == Move.State.Unavailable)
                {
                    var result = move.ChangeState(Move.State.Active, mute);
                    OnPerform.Invoke(move);
                    return result;
                }

                if (move.CurrentState == Move.State.Available)
                    return Perform(move, false, mute);

                return false;
            }
            else
            {
                if (move.CurrentState != Move.State.Available)
                    return false;

                var result = move.ChangeState(Move.State.Active, mute);
                OnPerform.Invoke(move);
                return result;
            }
        }

        /// <summary>
        /// Ends the first move of the specified type, if it's active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns>Whether the move was ended.</returns>
        public bool End<TMove>() where TMove : Move
        {
            return End(Moves.FirstOrDefault(m => m.CurrentState == Move.State.Active && m is TMove));
        }

        /// <summary>
        /// Ends the specified move if it's active.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <returns>Whether the move was ended.</returns>
        public bool End(Move move)
        {
            if (move == null || move.CurrentState != Move.State.Active) return false;

            move.ChangeState(move.Available ? Move.State.Available : Move.State.Unavailable);
            OnEnd.Invoke(move);

            return true;
        }

        /// <summary>
        /// Ends all moves.
        /// </summary>
        /// <param name="predicate">A move will be ended only if this predicate is true, if any.</param>
        /// <returns>If there were no active moves, always returns true. Otherwise, returns whether all
        /// moves were successfully ended.</returns>
        public bool EndAll(Predicate<Move> predicate = null)
        {
            var result = true;
            foreach (var move in new List<Move>(Moves))
            {
                if (move.CurrentState == Move.State.Active && predicate == null || predicate(move))
                    result &= End(move);
            }

            return result;
        }
        #endregion
        #region Powerup Helpers
        // Powerups are just objects that have moves on them. These objects can be added under
        // the manager's transform, at which point their moves will be added to the move list.

        /// <summary>
        /// Adds a copy of the specified powerup to the manager.
        /// </summary>
        /// <param name="powerup">The specified powerup to copy.</param>
        public void AddPowerup(GameObject powerup)
        {
            var p = Instantiate(powerup);
            p.transform.SetParent(Controller.transform);
            p.transform.position = Controller.transform.position;
            UpdateList();
        }

        /// <summary>
        /// Transfers the specified powerup to the manager.
        /// </summary>
        /// <param name="powerup">The specified powerup.</param>
        public void TransferPowerup(GameObject powerup)
        {
            powerup.transform.SetParent(Controller.transform);
            powerup.transform.position = Controller.transform.position;
            UpdateList();
        }

        /// <summary>
        /// Removes the specified powerup from the manager.
        /// </summary>
        /// <param name="powerup">The specified powerup.</param>
        public void RemovePowerup(GameObject powerup)
        {
            Destroy(powerup);
            UpdateList();
        }
        #endregion
    }
}
