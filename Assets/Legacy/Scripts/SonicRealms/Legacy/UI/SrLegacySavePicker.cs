using System.Linq;
using SonicRealms.Legacy.Game;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public class SrLegacySavePicker : MonoBehaviour
    {
        [SerializeField]
        private SrLegacySaveDataViewBase _saveIconPrefab;

        [SerializeField]
        private SrLegacyItemCarousel _saveIconCarousel;

        [SerializeField]
        private SrLegacySaveDataViewBase _saveInfoView;

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
