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
            GetComponents(Moves);
        }

        public void Awake()
        {
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
            {
                return;
            }

            // List copying is used abundantly since a move may remove itself or others from its own list,
            // causing iteration errors. There are faster ways to handle this, but at the moment the overhead
            // doesn't justify the effort.
            if (UnavailableMoves.Any())
            {
                foreach (var move in new List<Move>(UnavailableMoves))
                {
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
                    if (!move.Available())
                    {
                        if (!AvailableMoves.Remove(move))
                            continue;

                        UnavailableMoves.Add(move);
                        move.ChangeState(Move.State.Unavailable);

                        OnUnavailable.Invoke(move);
                    }
                    else if (move.InputActivate())
                    {
                        Perform(move);
                    }
                }
            }
            
            if (ActiveMoves.Any())
            {
                foreach (var move in new List<Move>(ActiveMoves))
                {
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
                move.OnActiveFixedUpdate();
            }
        }

        #region Move Helpers
        /// <summary>
        /// Returns the first move of the specified type.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public TMove Get<TMove>() where TMove : Move
        {
            return Moves.FirstOrDefault(move => move is TMove) as TMove;
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
    }
}
