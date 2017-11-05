using SonicRealms.Legacy.Game;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public abstract class SrLegacySaveDataViewBase : MonoBehaviour
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
