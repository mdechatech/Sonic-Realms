using System.Collections.Generic;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Powerups can be added to a player's powerup manager. Also, any moves attached to the powerup
    /// are automatically added to the player's move manager.
    /// </summary>
    public class Powerup : MonoBehaviour
    {
        /// <summary>
        /// How long the powerup lasts on a player in seconds. If less than or equal to zero, the powerup
        /// has no time limit.
        /// </summary>
        [Tooltip("How long the powerup lasts on a player in seconds. If less than or equal to zero, the powerup " +
                 "has no time limit.")]
        public float Duration;

        /// <summary>
        /// When this powerup is removed, powerups in this list will be removed as well.
        /// </summary>
        [Tooltip("When this powerup is removed, powerups in this list will be removed as well.")]
        public List<Powerup> Dependents;

            /// <summary>
        /// The animator to target. If left empty, it will be automatically set to the possessor's animator.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("The animator to target. If left empty, it will be automatically set to the possessor's animator.")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator bool to set to true when the powerup has an owner, false otherwise.
        /// </summary>
        [Foldout("Animation")]
        [Tooltip("Name of an Animator bool to set to true when the powerup has an owner, false otherwise.")]
        public string AddedBool;
        protected int AddedBoolHash;

        /// <summary>
        /// Invoked when the powerup is added to a player.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnAdd;

        /// <summary>
        /// Invoked when the powerup is added to a player.
        /// </summary>
        [Foldout("Events")]
        public UnityEvent OnRemove;

        [Foldout("Debug")] public HedgehogController Controller;
        [Foldout("Debug")] public PowerupManager Manager;
        [Foldout("Debug")] public MoveManager MoveManager;

        /// <summary>
        /// If the powerup has a duration, the time remaining until it is lost.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("If the powerup has a duration, the time remaining until it is lost.")]
        public float TimeRemaining;

        public virtual void Reset()
        {
            Dependents = new List<Powerup>();
            Animator = null;
            AddedBool = "";
            OnAdd = new UnityEvent();
            OnRemove = new UnityEvent();
            Duration = -1f;
        }

        public virtual void Awake()
        {
            AddedBoolHash = Animator.StringToHash(AddedBool);
            TimeRemaining = 0f;

            OnAdd = OnAdd ?? new UnityEvent();
            OnRemove = OnRemove ?? new UnityEvent();
        }

        public virtual void Start()
        {

        }

        public virtual void Update()
        {
            if (Manager == null)
                return;

            OnAddedUpdate();

            if (TimeRemaining <= 0f) return;
            if ((TimeRemaining -= Time.deltaTime) < 0f)
            {
                TimeRemaining = 0f;
                Remove();
            }
        }

        public virtual void FixedUpdate()
        {
            if (Manager == null) return;
            OnAddedFixedUpdate();
        }

        public void Add(PowerupManager manager)
        {
            manager.Add(this);
        }

        public void Remove()
        {
            if (Manager != null)
                Manager.Remove(this);
        }

        public void NotifyManagerAdd(PowerupManager manager)
        {
            Manager = manager;
            MoveManager = manager.MoveManager;
            Controller = manager.Controller;
            if (Controller != null && Animator == null) Animator = Controller.Animator;
            if (Duration > 0f) TimeRemaining = Duration;
            
            if (Animator != null && AddedBoolHash != 0)
            {
                var logWarnings = Animator.logWarnings;
                Animator.logWarnings = false;
                Animator.SetBool(AddedBoolHash, true);
                Animator.logWarnings = logWarnings;
            }

            OnManagerAdd();
            OnAdd.Invoke();
        }

        public void NotifyManagerRemove(PowerupManager manager)
        {
            if (Animator != null && AddedBoolHash != 0)
            {
                var logWarnings = Animator.logWarnings;
                Animator.logWarnings = false;
                Animator.SetBool(AddedBoolHash, false);
                Animator.logWarnings = logWarnings;
            }

            if (Dependents.Count > 0)
            {
                foreach(var dependent in Dependents)
                    if(dependent.Manager == Manager) dependent.Remove();
            }

            OnManagerRemove();

            Manager = null;
            MoveManager = null;
            Controller = null;

            OnRemove.Invoke();
            Destroy(this);
        }

        public virtual void OnManagerAdd()
        {

        }

        public virtual void OnManagerRemove()
        {

        }

        public virtual void OnAddedUpdate()
        {

        }

        public virtual void OnAddedFixedUpdate()
        {

        }
    }
}
