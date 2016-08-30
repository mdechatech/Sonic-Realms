using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Level
{
    public abstract class LevelManager : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent OnLevelComplete;

        public UnityEvent InitLevelCalled;

        public UnityEvent StartLevelCalled;

        public UnityEvent FinishLevelCalled;

        public virtual void Reset()
        {
            OnLevelComplete = new UnityEvent();
        }

        public virtual void Awake()
        {
            OnLevelComplete = OnLevelComplete ?? new UnityEvent();
        }

        public virtual void UpdateSave(SaveData data)
        {

        }

        public void InitLevel()
        {
            OnInitLevel();
            InitLevelCalled.Invoke();
        }

        public void StartLevel()
        {
            OnStartLevel();
            StartLevelCalled.Invoke();
        }

        public void FinishLevel()
        {
            OnFinishLevel();
            FinishLevelCalled.Invoke();
        }

        protected abstract void OnInitLevel();
        protected abstract void OnStartLevel();
        protected abstract void OnFinishLevel();
    }
}
