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

        /// <summary>
        /// The moves currently being performed.
        /// </summary>
        [Tooltip("The moves currently being performed.")]
        public List<Move> ActiveMoves;

        /// <summary>
        /// The moves which can be performed given the controller's current state.
        /// </summary>
        [Tooltip("The moves which can be performed given the controller's current state.")]
        public List<Move> AvailableMoves;

        /// <summary>
        /// The moves which cannot be performed given the controller's current state.
        /// </summary>
        [Tooltip("The moves which cannot be performed given the controller's current state.")]
        public List<Move> UnavailableMoves;

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
        #endregion

        public void Reset()
        {
            Controller = GetComponentInParent<HedgehogController>();
            GetComponentsInChildren(Moves);
        }

        public void Awake()
        {
            GetComponentsInChildren(Moves);

            ActiveMoves = new List<Move>();
            AvailableMoves = new List<Move>();
            UnavailableMoves = new List<Move>(Moves);

            OnPerform = OnPerform ?? new MoveEvent();
            OnInterrupted = OnInterrupted ?? new MoveEvent();
            OnEnd = OnEnd ?? new MoveEvent();
            OnAvailable = OnAvailable ?? new MoveEvent();
            OnUnavailable = OnUnavailable ?? new MoveEvent();
        }

        public void Update()
        {
            if (Controller.Interrupted)
                return;

            // List copying is used abundantly since a move may remove itself or others from its own list,
            // causing iteration errors. There are faster ways to handle this, but at the moment the overhead
            // doesn't justify the effort.
            if (UnavailableMoves.Any())
            {
                foreach (var move in new List<Move>(UnavailableMoves))
                {
                    // If the move is null or destroyed we need to remove it and update the list
                    if (!move)
                    {
                        UpdateList();
                        continue;
                    }

                    if (!move.Available())
                        continue;

                    if (!UnavailableMoves.Remove(move))
                        continue;

                    AvailableMoves.Add(move);
                    move.ChangeState(Move.State.Available);

                    OnAvailable.Invoke(move);
                }
            }
            
            if (AvailableMoves.Any())
            {
                foreach (var move in new List<Move>(AvailableMoves))
                {
                    // If the move is null or destroyed we need to remove it and update the list
                    if (!move)
                    {
                        UpdateList();
                        continue;
                    }

                    if (!move.Available())
                    {
                        if (!AvailableMoves.Remove(move))
                            continue;

                        UnavailableMoves.Add(move);
                        move.ChangeState(Move.State.Unavailable);

                        OnUnavailable.Invoke(move);
                    }
                    else if (move.InputEnabled && move.InputActivate())
                    {
                        Perform(move);
                    }
                }
            }
            
            if (ActiveMoves.Any())
            {
                foreach (var move in new List<Move>(ActiveMoves))
                {
                    // If the move is null or destroyed we need to remove it and update the list
                    if (!move)
                    {
                        UpdateList();
                        continue;
                    }

                    if (move.InputDeactivate())
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

            foreach (var move in new List<Move>(ActiveMoves))
            {
                // Might as well check to see if the move has been destroyed - if it has, update the list
                if (!move)
                {
                    UpdateList();
                    continue;
                }

                move.OnActiveFixedUpdate();
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
            Moves.RemoveAll(move =>
            {
                if (move && move.transform.IsChildOf(transform)) return false;

                switch (move.CurrentState)
                {
                    case Move.State.Active:
                        ActiveMoves.Remove(move);
                        break;

                    case Move.State.Available:
                        AvailableMoves.Remove(move);
                        break;

                    case Move.State.Unavailable:
                        UnavailableMoves.Remove(move);
                        break;
                }

                move.OnManagerRemove();
                move.OnRemove.Invoke();
                return true;
            });

            // Copy the current list before filling it with new moves
            var moves = new List<Move>(Moves);
            GetComponentsInChildren(Moves);
            
            // Compare the copied list with the new, add/notify the ones they don't have in common
            foreach (var move in Moves.Where(move => !moves.Contains(move)))
            {
                move.Controller = Controller;
                move.Manager = this;

                move.ChangeState(Move.State.Unavailable);
                UnavailableMoves.Add(move);

                move.OnManagerAdd();
                move.OnAdd.Invoke();
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
            return AvailableMoves.Any(move => move is TMove);
        }

        /// <summary>
        /// Returns whether the first move of the specified type is active.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool IsActive<TMove>() where TMove : Move
        {
            return ActiveMoves.Any(move => move is TMove);
        }

        /// <summary>
        /// Performs the first move of the specified type, if it's available.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <param name="force">Whether to perform the move even if it's unavailable.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool Perform<TMove>(bool force = false) where TMove : Move
        {
            var foundMove = AvailableMoves.FirstOrDefault(move => move is TMove);

            if (force && foundMove == null)
                foundMove = UnavailableMoves.FirstOrDefault(move => move is TMove);

            if (foundMove == null)
                return false;

            return Perform(foundMove, force);
        }

        /// <summary>
        /// Performs the first move of the specified name (invocable from the UI).
        /// </summary>
        /// <param name="moveType"></param>
        public void Perform(string moveType)
        {
            Perform(Moves.FirstOrDefault(move => move.GetType().Name == moveType));
        }

        /// <summary>
        /// Performs the specified move, if it's available.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <param name="force">Whether to perform the move even if it's unavailable.</param>
        /// <returns>Whether the move was performed.</returns>
        public bool Perform(Move move, bool force = false)
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
                    if (!UnavailableMoves.Remove(move))
                        return false;
                    
                    ActiveMoves.Add(move);
                    var result = move.ChangeState(Move.State.Active);
                    OnPerform.Invoke(move);
                    return result;
                }
                else if (move.CurrentState == Move.State.Available)
                {
                    return Perform(move, false);
                }

                return false;
            }
            else
            {
                if (move.CurrentState != Move.State.Available)
                    return false;

                if (!AvailableMoves.Remove(move))
                    return false;

                ActiveMoves.Add(move);
                var result = move.ChangeState(Move.State.Active);
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
            return End(ActiveMoves.FirstOrDefault(m => m is TMove));
        }

        /// <summary>
        /// Ends the specified move if it's active.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <returns>Whether the move was ended.</returns>
        public bool End(Move move)
        {
            if (move == null || move.CurrentState != Move.State.Active)
                return false;

            if (!ActiveMoves.Remove(move))
                return false;

            if (move.Available())
            {
                AvailableMoves.Add(move);
                move.ChangeState(Move.State.Available);
            }
            else
            {
                UnavailableMoves.Add(move);
                move.ChangeState(Move.State.Unavailable);
            }

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
            if (!ActiveMoves.Any())
                return true;

            var result = true;
            foreach (var move in new List<Move>(ActiveMoves))
            {
                if (predicate == null)
                {
                    result &= End(move);
                }
                else
                {
                    if (predicate(move))
                        result &= End(move);
                }
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
