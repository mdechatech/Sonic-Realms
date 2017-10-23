using System.Collections.Generic;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using SonicRealms.Legacy.Game;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public class SrLegacyResolutionPicker : MonoBehaviour
    {
        [SerializeField, SrFoldout("Data")]
        private bool _useGameManagerResolutions;

        [SerializeField, SrFoldout("Data")]
        private SrLegacyResolutionSettings _resolutions;

        [SerializeField, SrFoldout("Prefabs")]
        private SrLegacyResolutionPickerAspect _aspectEntryPrefab;
        
        [SerializeField, SrFoldout("Prefabs")]
        private SrLegacyResolutionPickerScreenSize _screenSizeEntryPrefab;

        [SerializeField, SrFoldout("Prefabs")]
        private SrLegacyResolutionPickerFullscreen _fullscreenEntryPrefab;

        [SerializeField, SrFoldout("Components")]
        private SrLegacyItemCarousel _aspectCarousel;

        [SerializeField, SrFoldout("Components")]
        private SrLegacyItemCarousel _screenSizeCarousel;

        [SerializeField, SrFoldout("Components")]
        private SrLegacyItemCarousel _fullscreenCarousel;

        private Resolution _maxResolution;

        private bool _selectedFullscreen;
        private SrLegacyResolutionSettings.Entry _selectedEntry;

        private List<SrLegacyResolutionPickerAspect> _aspectEntries;
        private SrMap<int, int> _aspectIndexMap; // Carousel index -> settings index

        private List<SrLegacyResolutionPickerScreenSize> _screenSizeEntries;
        private SrMap<int, int> _screenSizeIndexMap; // Carousel index -> settings index


        protected virtual void Reset()
        {
            _useGameManagerResolutions = true;
#if UNITY_EDITOR
            _resolutions = UnityEditor.AssetDatabase.LoadAssetAtPath<SrLegacyResolutionSettings>(
                "Assets/Data/Resolutions/Genesis Resolutions.asset");
#endif
        }

        protected virtual void Awake()
        {
            _aspectEntries = new List<SrLegacyResolutionPickerAspect>();
            _aspectIndexMap = new SrMap<int, int>();

            _screenSizeEntries = new List<SrLegacyResolutionPickerScreenSize>();
            _screenSizeIndexMap = new SrMap<int, int>();
        }

        protected virtual void Start()
        {
            GetHighestResolution();
            SetResolutionChoices();

            PopulateAspectCarousel();
            PopulateScreenSizeCarousel();
            PopulateFullscreenCarousel();

            _fullscreenCarousel.OnSelectionChange.AddListener(e =>
            {
                _selectedFullscreen = e.NewIndex == 1;

                PopulateAspectCarousel();
                PopulateScreenSizeCarousel();
            });

            _aspectCarousel.OnSelectionChange.AddListener(e =>
            {
                PopulateScreenSizeCarousel();
            });

            _screenSizeCarousel.OnSelectionChange.AddListener(e =>
            {
                if (_selectedFullscreen)
                {
                    _selectedEntry = _resolutions.GetFullscreenEntry(
                        _screenSizeIndexMap.Forward[_screenSizeCarousel.SelectedIndex]).ToEntry();
                }
                else
                {
                    _selectedEntry = _resolutions.GetWindowedEntry(
                        _aspectIndexMap.Forward[_aspectCarousel.SelectedIndex])
                        .ToEntry(_screenSizeIndexMap.Forward[_screenSizeCarousel.SelectedIndex]);
                }

                Screen.SetResolution(_selectedEntry.Resolution.Width, _selectedEntry.Resolution.Height,
                    _selectedEntry.Fullscreen);
            });
        }

        private void GetHighestResolution()
        {
            var max = default(Resolution);

            for (var i = 0; i < Screen.resolutions.Length; ++i)
            {
                var resolution = Screen.resolutions[i];

                if (max.width*max.height < resolution.width*resolution.height)
                    max = resolution;
            }

            _maxResolution = max;
        }

        private void SetResolutionChoices()
        {
            if (_useGameManagerResolutions)
            {
                if (SrLegacyGameManager.Instance && SrLegacyGameManager.Instance.ResolutionChoices)
                {
                    _resolutions = SrLegacyGameManager.Instance.ResolutionChoices;
                }
                else
                {
                    Debug.LogError("'Use Game Manager Resolutions' is checked, but the Game Manager doesn't exist " +
                                   "or doesn't have anything set for 'Resolution Choices'.");
                }
            }
        }

        private void PopulateAspectCarousel()
        {
            _aspectCarousel.Clear();
            _aspectEntries.Clear();
            _aspectIndexMap.Clear();

            if (_selectedFullscreen)
            {
                // TODO disable
            }
            else
            {
                for (var i = 0; i < _resolutions.WindowedEntryCount; ++i)
                {
                    var entry = Instantiate(_aspectEntryPrefab);
                    entry.Value = _resolutions.GetWindowedEntry(i).AspectRatio;

                    _aspectEntries.Add(entry);
                    _aspectIndexMap.Add(_aspectCarousel.ItemCount, i);
                    _aspectCarousel.Add(entry.gameObject);
                }
            }
        }

        private void PopulateScreenSizeCarousel()
        {
            _screenSizeCarousel.Clear();
            _screenSizeEntries.Clear();
            _screenSizeIndexMap.Clear();

            if (_selectedFullscreen)
            {
                for (var i = 0; i < _resolutions.FullscreenEntryCount; ++i)
                {
                    var size = _resolutions.GetFullscreenEntry(i);

                    if (size.Resolution.Width > _maxResolution.width || size.Resolution.Height > _maxResolution.height)
                        continue;

                    var entry = Instantiate(_screenSizeEntryPrefab);
                    entry.Value = size.Resolution;

                    _screenSizeEntries.Add(entry);
                    _screenSizeIndexMap.Add(_screenSizeCarousel.ItemCount, i);
                    _screenSizeCarousel.Add(entry.gameObject);
                }
            }
            else
            {
                var sizes = _resolutions.GetWindowedEntry(_aspectCarousel.SelectedIndex);
                for (var i = 0; i < sizes.ScreenSizeCount; ++i)
                {
                    var size = sizes[i];

                    if (size.Width > _maxResolution.width || size.Height > _maxResolution.height)
                        continue;

                    var entry = Instantiate(_screenSizeEntryPrefab);
                    entry.Value = size;

                    _screenSizeEntries.Add(entry);
                    _screenSizeIndexMap.Add(_screenSizeCarousel.ItemCount, i);
                    _screenSizeCarousel.Add(entry.gameObject);
                }
            }
        }

        private void PopulateFullscreenCarousel()
        {
            _fullscreenCarousel.Clear();

            var offEntry = Instantiate(_fullscreenEntryPrefab);
            offEntry.Value = false;

            var onEntry = Instantiate(_fullscreenEntryPrefab);
            onEntry.Value = true;

            _fullscreenCarousel.Add(offEntry.gameObject);
            _fullscreenCarousel.Add(onEntry.gameObject);
        }
    }
}
