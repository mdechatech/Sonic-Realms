using SonicRealms.Legacy.Game;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public class SavePickerIconView : SaveDataViewBase
    {
        [SerializeField]
        private GameObject _iconContainer;

        [SerializeField]
        private GameObject _defaultIconPrefab;

        private GameObject _icon;

        protected virtual void Reset()
        {
            _iconContainer = gameObject;
        }

        protected override void UpdateSaveData(SaveData oldValue, SaveData newValue)
        {
            if (newValue == null)
            {
                SetIcon(_defaultIconPrefab);
                return;
            }

            var levelData = GameManager.Instance.GetLevelData(newValue.Level);

            if (levelData == null || !levelData.LevelSelectIcon)
            {
                SetIcon(_defaultIconPrefab);
            }
            else
            {
                SetIcon(levelData.LevelSelectIcon);
            }
        }

        private void SetIcon(GameObject iconPrefab)
        {
            if (_icon)
                Destroy(_icon);

            if (iconPrefab)
            {
                _icon = (GameObject) Instantiate(iconPrefab, _iconContainer.transform);
            }
        }
    }
}
