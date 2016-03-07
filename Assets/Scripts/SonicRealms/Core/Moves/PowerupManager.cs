using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Actors;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Core.Moves
{
    public class PowerupManager : MonoBehaviour
    {
        public HedgehogController Controller;
        public MoveManager MoveManager;
        
        [Foldout("Events")] public PowerupEvent OnAdd;
        [Foldout("Events")] public PowerupEvent OnRemove;

        [Foldout("Debug")] public List<Powerup> Powerups;

        public void Reset()
        {
            Controller = GetComponent<HedgehogController>();
            MoveManager = GetComponent<MoveManager>();
            OnAdd = new PowerupEvent();
            OnRemove = new PowerupEvent();
        }

        public void Awake()
        {
            Controller = Controller ?? GetComponent<HedgehogController>();
            MoveManager = GetComponent<MoveManager>();
            Powerups = new List<Powerup>();
            OnAdd = OnAdd ?? new PowerupEvent();
            OnRemove = OnRemove ?? new PowerupEvent();
        }

        public void Start()
        {
            GetComponentsInChildren(Powerups);
        }

        public void Add(Powerup powerup)
        {
            if(powerup == null || Has(powerup)) return;

            NotifyAdd(powerup);

            powerup.transform.SetParent(transform);
            powerup.transform.position = transform.position;

            var extras = powerup.GetComponents<Powerup>();
            foreach (var extra in extras)
            {
                if(extra != powerup)
                    NotifyAdd(extra);
            }
        }

        public void Add(GameObject powerupContainer)
        {
            var powerup = powerupContainer.GetComponent<Powerup>();
            if (powerup == null)
            {
                Debug.LogWarning(string.Format("Powerup container '{0}' has no powerups on the object.",
                    powerupContainer.name));
                return;
            }

            Add(powerup);
        }

        protected void NotifyAdd(Powerup powerup)
        {
            Powerups.Add(powerup);
            powerup.NotifyManagerAdd(this);
            OnAdd.Invoke(powerup);
        }

        public bool Remove(Powerup powerup)
        {
            var result = Powerups.Remove(powerup);
            if (!result) return false;

            powerup.NotifyManagerRemove(this);
            OnRemove.Invoke(powerup);

            var go = powerup.gameObject;
            Destroy(powerup);

            var powerups = go.GetComponents<Powerup>();
            if (powerups.Length == 0 || powerups.All(powerup1 => powerup1.Manager != this))
                Destroy(go);

            return true;
        }

        public bool Remove(GameObject container)
        {
            if (container == null || !container.transform.IsChildOf(transform)) return false;
            foreach (var powerup in container.GetComponents<Powerup>()) Remove(powerup);
            return true;
        }

        public bool Remove<TPowerup>() where TPowerup : Powerup
        {
            return Remove(Powerups.FirstOrDefault(powerup => powerup is TPowerup));
        }

        public TPowerup Get<TPowerup>() where TPowerup : Powerup
        {
            return Powerups.FirstOrDefault(powerup => powerup is TPowerup) as TPowerup;
        }

        public IEnumerable<TPowerup> GetAll<TPowerup>() where TPowerup : Powerup
        {
            return Powerups.OfType<TPowerup>();
        }

        public bool Has(Powerup powerup)
        {
            return Powerups.Contains(powerup);
        }

        public bool Has<TPowerup>() where TPowerup : Powerup
        {
            return Powerups.Any(powerup => powerup is TPowerup);
        }
    }
}
