using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Keeps track of player lives.
    /// </summary>
    public class LifeCounter : MonoBehaviour
    {
        [SerializeField]
        private int _lives;
        public int Lives
        {
            get { return _lives; }
            set
            {
                if (_lives == value) return;

                _lives = value;
                if (_lives < 0) _lives = 0;
                OnValueChange.Invoke();
            }
        }

        public UnityEvent OnValueChange;

        public void Awake()
        {
            OnValueChange = OnValueChange ?? new UnityEvent();
        }
    }
}
