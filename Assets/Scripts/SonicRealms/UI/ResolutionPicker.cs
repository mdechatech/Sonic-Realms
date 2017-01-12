using System.Collections.Generic;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.UI
{
    public class ResolutionPicker : MonoBehaviour
    {
        [SerializeField, Foldout("Data")]
        private bool _useGameManagerResolutions;

        [SerializeField, Foldout("Data")]
        private ResolutionSettings _resolutions;

        [SerializeField, Foldout("Prefabs")]
        private ResolutionPickerAspect _aspectEntryPrefab;
        
        [SerializeField, Foldout("Prefabs")]
        private ResolutionPickerScreenSize _screenSizeEntryPrefab;

        [SerializeField, Foldout("Prefabs")]
        private ResolutionPickerFullscreen _fullscreenEntryPrefab;

        [SerializeField, Foldout("Components")]
        private ItemCarousel _aspectCarousel;

        [SerializeField, Foldout("Components")]
        private ItemCarousel _screenSizeCarousel;

        [SerializeField, Foldout("Components")]
        private ItemCarousel _fullscreenCarousel;

        private Resolution _maxResolution;

        private bool _selectedFullscreen;
        private ResolutionSettings.Entry _selectedEntry;

        private List<ResolutionPickerAspect> _aspectEntries;
        private Map<int, int> _aspectIndexMap; // Carousel index -> settings index

        private List<ResolutionPickerScreenSize> _screenSizeEntries;
        private Map<int, int> _screenSizeIndexMap; // Carousel index -> settings index


        protected virtual void Reset()
        {
            _useGameManagerResolutions = true;
#if UNITY_EDITOR
            _resolutions = UnityEditor.AssetDatabase.LoadAssetAtPath<ResolutionSettings>(
                "Assets/Data/Resolutions/Genesis Resolutions.asset");
#endif
        }

        protected virtual void Awake()
        {
            _aspectEntries = new List<ResolutionPickerAspect>();
            _aspectIndexMap = new Map<int, int>();

            _screenSizeEntries = new List<ResolutionPickerScreenSize>();
            _screenSizeIndexMap = new Map<int, int>();
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
                if (GameManager.Instance && GameManager.Instance.ResolutionChoices)
                {
                    _resolutions = GameManager.Instance.ResolutionChoices;
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
