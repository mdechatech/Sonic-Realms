using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Level
{
    public abstract class LevelManager : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent OnLevelComplete;

        public virtual void Reset()
        {
            OnLevelComplete = new UnityEvent();
        }

        public virtual void Awake()
        {
            OnLevelComplete = OnLevelComplete ?? new UnityEvent();
        }

        public abstract void InitLevel();
        public abstract void StartLevel();
        public abstract void FinishLevel();

        public virtual void UpdateSave(SaveData data)
        {

        }
    }
}
