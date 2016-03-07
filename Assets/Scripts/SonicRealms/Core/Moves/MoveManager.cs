using System;
using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
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
        /// The active move on each layer, or null if the layer has no active move.
        /// </summary>
        public Dictionary<MoveLayer, Move> Layers;

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
            Moves = new List<Move>();

            InitializeLayers();
        }

        public void Start()
        {
            foreach (var move in GetComponentsInChildren<Move>()) NotifyAdd(move);
        }

        private void InitializeLayers()
        {
            Layers = new Dictionary<MoveLayer, Move>();
            foreach (var layer in Enum.GetValues(typeof(MoveLayer)).Cast<MoveLayer>())
            {
                Layers.Add(layer, null);
            }
        }

        public void Update()
        {
            if (Controller.Interrupted)
                return;

            for (var i = Moves.Count - 1; i >= 0; i = --i >= Moves.Count ? Moves.Count - 1 : i)
                HandleState(Moves[i]);
        }

        public void HandleState(Move move)
        {
            if (!move.enabled || !move.gameObject.activeInHierarchy) return;
            if (move.CurrentState == Move.State.Unavailable)
            {
                if (!move.Available) return;

                move.ChangeState(Move.State.Available);
                OnAvailable.Invoke(move);
            }

            if (move.CurrentState == Move.State.Available)
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
            }
        }

        #region Move Helpers
        /// <summary>
        /// Adds the specified move to the moveset.
        /// </summary>
        /// <param name="move">The specified move.</param>
        public void Add(Move move)
        {
            if (move == null || Has(move)) return;
            
            NotifyAdd(move);

            move.transform.SetParent(transform);
            move.transform.position = transform.position;

            var extras = move.GetComponents<Move>();
            foreach (var extra in extras)
            {
                if (extra != move)
                    NotifyAdd(move);
            }
        }

        /// <summary>
        /// Adds all moves on the given object to the moveset.
        /// </summary>
        /// <param name="container">The given object. Moves on children of the object will not be added.</param>
        public void Add(GameObject container)
        {
            var move = container.GetComponent<Move>();
            if (move == null)
            {
                Debug.LogWarning(string.Format("Move container '{0}' has no moves on the object.", 
                    container.name));
                return;
            }

            Add(move);
        }

        protected void NotifyAdd(Move move)
        {
            Moves.Add(move);
            move.NotifyManagerAdd(this);
            OnAdd.Invoke(move);
        }

        /// <summary>
        /// Removes the specified move from the moveset.
        /// </summary>
        /// <param name="move">The specified move.</param>
        public bool Remove(Move move)
        {
            var result = Moves.Remove(move);
            if (!result) return false;

            move.NotifyManagerRemove(this);
            OnRemove.Invoke(move);

            var go = move.gameObject;
            Destroy(move);

            var moves = go.GetComponents<Move>();
            if (moves.Length == 0 || moves.All(move1 => move1.Manager != this))
                Destroy(go);

            return true;
        }

        /// <summary>
        /// Removes the specified object and all of its moves from the moveset.
        /// </summary>
        /// <param name="container">The specified object. </param>
        /// <returns></returns>
        public bool Remove(GameObject container)
        {
            if (container == null || !container.transform.IsChildOf(transform)) return false;
            foreach (var move in container.GetComponents<Move>()) Remove(move);
            return true;
        }

        /// <summary>
        /// Returns whether the manager has the specified move.
        /// </summary>
        /// <param name="move">The specified move.</param>
        /// <returns></returns>
        public bool Has(Move move)
        {
            return Moves.Contains(move);
        }

        /// <summary>
        /// Returns whether the manager has a move of the specified type.
        /// </summary>
        /// <typeparam name="TMove">The specified type.</typeparam>
        /// <returns></returns>
        public bool Has<TMove>() where TMove : Move
        {
            return Moves.Any(move => move is TMove);
        }

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
        /// Returns the active move on the specified layer, if any.
        /// </summary>
        /// <param name="layer">The specified layer.</param>
        /// <returns></returns>
        public Move Get(MoveLayer layer)
        {
            return Layers[layer];
        }

        /// <summary>
        /// The layer's active move, if any. If setting, the given move will be performed as long as it's on
        /// the given layer.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public Move this[MoveLayer layer]
        {
            get { return Get(layer); }
            set { if (value.Layer == layer) Perform(value); }
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

        /// <summary>
        /// Performs the specified move.
        /// </summary>
        /// <param name="move">The specified move.</param>
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
                    if (move.Layer != MoveLayer.None)
                    {
                        End(Layers[move.Layer]);
                        Layers[move.Layer] = move;
                    }

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

                if (move.Layer != MoveLayer.None)
                {
                    End(Layers[move.Layer]);
                    Layers[move.Layer] = move;
                }

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

            if(move.Layer != MoveLayer.None) Layers[move.Layer] = null;

            move.ChangeState(move.Available ? Move.State.Available : Move.State.Unavailable);
            OnEnd.Invoke(move);

            return true;
        }

        /// <summary>
        /// Ends the move on the specified layer, if any.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool End(MoveLayer layer)
        {
            return End(Layers[layer]);
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

        public void Enable<TMove>() where TMove : Move
        {
            var move = Get<TMove>();
            if (move == null) return;
            move.enabled = true;
        }

        public void Disable<TMove>() where TMove : Move
        {
            var move = Get<TMove>();
            if (move == null) return;
            move.enabled = false;
        }
        #endregion
    }
}
