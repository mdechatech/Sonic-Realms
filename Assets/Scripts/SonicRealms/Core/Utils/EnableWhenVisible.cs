using UnityEngine;

namespace SonicRealms.Core.Utils
{
    public class EnableWhenVisible : MonoBehaviour
    {
        public bool StartDisabled;
        public bool DisableWhenInvisible;

        private Behaviour[] _behaviours;

        public void Reset()
        {
            StartDisabled = true;
            DisableWhenInvisible = false;
        }

        public void Start()
        {
            _behaviours = GetComponentsInChildren<Behaviour>();
            AddCallbacks();

            if(StartDisabled) OnBecameInvisible();
        }

        public void AddCallbacks()
        {
            foreach (var t in transform)
            {
                var callback = (t as Transform).gameObject.AddComponent<OnBecameVisibleCallback>();
                callback.BecameVisible.AddListener(OnBecameVisible);
                if(DisableWhenInvisible) callback.BecameInvisible.AddListener(OnBecameInvisible);
            }
        }

        public void OnBecameVisible()
        {
            for (var i = 0; i < _behaviours.Length; ++i)
                _behaviours[i].enabled = true;
        }

        public void OnBecameInvisible()
        {
            for (var i = 0; i < _behaviours.Length; ++i)
                _behaviours[i].enabled = false;
        }
    }
}
