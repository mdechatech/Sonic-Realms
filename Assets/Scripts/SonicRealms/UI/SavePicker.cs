using System.Linq;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.UI
{
    public class SavePicker : MonoBehaviour
    {
        [SerializeField]
        private SaveDataViewBase _saveIconPrefab;

        [SerializeField]
        private ItemCarousel _saveIconCarousel;

        [SerializeField]
        private SaveDataViewBase _saveInfoView;

        [SerializeField]
        private string[] _saveFileNames;

        protected virtual void Reset()
        {
            _saveFileNames = Enumerable.Range(0, 10).Select(n => n.ToString()).ToArray();
        }

        protected virtual void Awake()
        {
            _saveFileNames = _saveFileNames ?? new string[0];
        }

        protected virtual void Start()
        {
            AddIcons();
        }

        private void AddIcons()
        {
            for (var i = 0; i < _saveFileNames.Length; ++i)
            {
                var fileName = _saveFileNames[i];

                var icon = Instantiate(_saveIconPrefab);
                _saveIconCarousel.Add(icon.gameObject);

                var data = SaveManager.Load(fileName);
                icon.SaveData = data;
            }
        }
    }
}
