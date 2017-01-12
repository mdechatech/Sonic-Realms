using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.UI
{
    public abstract class SaveDataViewBase : MonoBehaviour
    {
        private SaveData _saveData;

        public SaveData SaveData
        {
            get { return _saveData; }
            set
            {
                var old = _saveData;
                _saveData = value;

                UpdateSaveData(old, value);
            }
        }

        protected abstract void UpdateSaveData(SaveData oldValue, SaveData newValue);
    }
}
