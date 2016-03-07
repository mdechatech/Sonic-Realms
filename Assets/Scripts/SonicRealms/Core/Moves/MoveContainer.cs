using System.Collections.Generic;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    /// <summary>
    /// Powerup that, when gained, gives all the moves on this object to the player.
    /// </summary>
    public class MoveContainer : Powerup
    {
        /// <summary>
        /// When gained, the powerup will give all the moves on this object to the player.
        /// </summary>
        [Tooltip("When gained, the powerup will give all the moves on this object to the player.")]
        public GameObject Container;

        [HideInInspector] public List<Move> Moves;

        public override void Reset()
        {
            base.Reset();
            Container = gameObject;
        }

        public override void Awake()
        {
            base.Awake();
            Moves = new List<Move>();
            GetComponents(Moves);
        }

        public override void OnManagerAdd()
        {
            if (Manager.MoveManager == null) return;
            Manager.MoveManager.Add(Container);
        }

        public override void OnManagerRemove()
        {
            if (Manager.MoveManager == null) return;
            Manager.MoveManager.Remove(Container);
        }

        public void OnEnable()
        {
            Container.gameObject.SetActive(true);
        }

        public void OnDisable()
        {
            Container.gameObject.SetActive(false);
        }
    }
}
