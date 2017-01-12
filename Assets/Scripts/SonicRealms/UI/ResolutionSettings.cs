using System;
using System.Collections.Generic;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.UI
{
    [CreateAssetMenu(fileName = "Resolution Settings", menuName = "Sonic Realms/Resolution Settings", order = -1)]
    public class ResolutionSettings : ScriptableObject
    {
        /// <summary>
        /// Number of windowed resolution entries.
        /// </summary>
        public int WindowedEntryCount { get { return _windowedEntries.Count; } }

        /// <summary>
        /// Number of fullscreen resolution entries.
        /// </summary>
        public int FullscreenEntryCount { get { return _fullscreenEntries.Count; } }

        /// <summary>
        /// Whether to allow fullscreen resolutions. Resolution choices in fullscreen are not customizable, and they 
        /// vary between different machines.
        /// </summary>
        public bool EnableFullscreen { get { return _enableFullscreen; } set { _enableFullscreen = value; } }

        /// <summary>
        /// If checked, the first screen size in a windowed resolution entry will be used as the base resolution. 
        /// Cameras will render to a texture of that size then upscale it to fit the window bounds. Good for pixel 
        /// art, not so much for HD.
        /// </summary>
        public bool UseResolutionDownscaling { get { return _useResolutionDownscaling; } set { _useResolutionDownscaling = value; } }

        [SerializeField]
        [Tooltip("If checked, cameras will render to the Downscaled Resolution then size it up to fit the screen resolution.")]
        private bool _useResolutionDownscaling;

        [SerializeField]
        [Tooltip("Whether to allow fullscreen resolutions. Resolution choices in fullscreen are not customizable, and they " +
                 "vary between different machines.")]
        private bool _enableFullscreen;

        [SerializeField]
        private List<FullscreenEntry> _fullscreenEntries;

        [SerializeField]
        private List<WindowedEntry> _windowedEntries;
        
        protected void Reset()
        {
            _enableFullscreen = true;
        }

        public WindowedEntry GetWindowedEntry(int index)
        {
            return _windowedEntries[index];
        }

        public FullscreenEntry GetFullscreenEntry(int index)
        {
            return _fullscreenEntries[index];
        }

        [Serializable]
        public class Entry
        {
            public float CameraOrthographicSize
            {
                get { return _cameraOrthographicSize; }
                set { _cameraOrthographicSize = value; }
            }

            public ScreenSize DownscaledResolution
            {
                get { return _downscaledResolution; }
                set { _downscaledResolution = value; }
            }

            public ScreenSize Resolution { get { return _resolution; } set { _resolution = value; } }

            public bool Fullscreen { get { return _fullscreen; } set { _fullscreen = value; } }

            [SerializeField]
            private float _cameraOrthographicSize;

            [SerializeField]
            private ScreenSize _downscaledResolution;

            [SerializeField]
            private ScreenSize _resolution;

            [SerializeField]
            private bool _fullscreen;

            public Entry(float cameraOrthographicSize, ScreenSize downscaledResolution, ScreenSize resolution,
                bool fullscreen)
            {
                _cameraOrthographicSize = cameraOrthographicSize;
                _downscaledResolution = downscaledResolution;
                _resolution = resolution;
                _fullscreen = fullscreen;
            }
        }

        [Serializable]
        public class FullscreenEntry
        {
            public float CameraOrthographicSize
            {
                get { return _cameraOrthographicSize; }
                set { _cameraOrthographicSize = value; }
            }

            public ScreenSize DownscaledResolution { get { return _downscaledResolution; } }

            public ScreenSize Resolution { get { return _resolution; } }

            [SerializeField]
            [Tooltip("The orthographic size of cameras in-game when using this resolution.")]
            private float _cameraOrthographicSize;

            [SerializeField]
            [Tooltip("If resolution downscaling is used, the size from which the game gets upscaled to the " +
                     "Desired Resolution.")]
            private ScreenSize _downscaledResolution;

            [SerializeField]
            [Tooltip("The desired screen size. This must match one of the machine's available fullscreen resolutions, " +
                     "or it won't show up as an option. Safe choices are: 640x480, 800x600, ")]
            private ScreenSize _resolution;

            public FullscreenEntry(float cameraOrthographicSize, ScreenSize downscaledResolution, ScreenSize resolution)
            {
                _cameraOrthographicSize = cameraOrthographicSize;
                _downscaledResolution = downscaledResolution;
                _resolution = resolution;
            }

            public Entry ToEntry()
            {
                return new Entry(_cameraOrthographicSize, _downscaledResolution, _resolution, true);
            }
        }

        [Serializable]
        public class WindowedEntry
        {
            public AspectRatio AspectRatio { get { return new AspectRatio(AspectHorizontal, AspectVertical); } }

            public int AspectHorizontal { get { return _aspectRatio.Horizontal; } }
            public int AspectVertical { get { return _aspectRatio.Vertical; } }

            public int ScreenSizeCount { get { return _screenSizes.Count; } }

            public float CameraOrthographicSize { get { return _cameraOrthographicSize; } }

            public ScreenSize DownscaledResolution { get { return _downscaledResolution; } set { _downscaledResolution = value; } }

            public ScreenSize this[int index] { get { return _screenSizes[index]; } }

            [SerializeField]
            private float _cameraOrthographicSize;

            [SerializeField]
            private AspectRatio _aspectRatio;

            [SerializeField]
            private ScreenSize _downscaledResolution;

            [SerializeField]
            private List<ScreenSize> _screenSizes;

            public WindowedEntry(float cameraOrthographicSize, AspectRatio aspectRatio, int firstScreenHeight)
            {
                _cameraOrthographicSize = cameraOrthographicSize;
                _aspectRatio = aspectRatio;
                _screenSizes = new List<ScreenSize>();

                AddScreenSize(firstScreenHeight);
            }

            public void AddScreenSize(int height)
            {
                var screenSize = new ScreenSize(0, height);
                screenSize.Ceil(_aspectRatio);

                _screenSizes.Add(screenSize);
            }

            public Entry ToEntry(int index)
            {
                return new Entry(_cameraOrthographicSize, _downscaledResolution, _screenSizes[index], false);
            }
        }

        [Serializable]
        public class AspectRatio : IEquatable<AspectRatio>
        {
            public int Horizontal { get { return _horizontal; } set { _horizontal = Mathf.Max(1, value); } }
            public int Vertical { get { return _vertical; } set { _vertical = Mathf.Max(1, value); } }

            public float Value { get { return Horizontal/(float) Vertical; } }

            [SerializeField]
            private int _horizontal;
            
            [SerializeField]
            private int _vertical;

            public AspectRatio(int horizontal, int vertical)
            {
                Horizontal = horizontal;
                Vertical = vertical;
            }

            public AspectRatio GetReduced()
            {
                var gcd = DMath.GCD(Horizontal, Vertical);
                return new AspectRatio(Horizontal/gcd, Vertical/gcd);
            }

            public bool Equals(AspectRatio other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;

                var reduced = GetReduced();
                var otherReduced = other.GetReduced();

                return reduced._horizontal == otherReduced._horizontal &&
                       reduced._vertical == otherReduced._vertical;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((AspectRatio) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_horizontal*397) ^ _vertical;
                }
            }

            public static bool operator ==(AspectRatio left, AspectRatio right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(AspectRatio left, AspectRatio right)
            {
                return !Equals(left, right);
            }
        }

        [Serializable]
        public class ScreenSize : IEquatable<ScreenSize>
        {
            public int Width { get { return _width; } set { _width = Mathf.Max(1, value); } }
            public int Height { get { return _height; } set { _height = Mathf.Max(1, value); } }

            [SerializeField]
            private int _width;

            [SerializeField]
            private int _height;

            public ScreenSize(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public void NextHighest(AspectRatio aspect)
            {
                ++Height;

                Ceil(aspect);
            }

            public void NextLowest(AspectRatio aspect)
            {
                --Height;

                Floor(aspect);
            }

            public void Ceil(AspectRatio aspect)
            {
                var reducedRatio = aspect.GetReduced();

                var roundedHeight = Height + (reducedRatio.Vertical - Height%reducedRatio.Vertical - 1);

                Height = roundedHeight;
                Width = Mathf.RoundToInt(Height * aspect.Value);
            }

            public void Floor(AspectRatio aspect)
            {
                var reducedRatio = aspect.GetReduced();

                var roundedHeight = Height - Height%reducedRatio.Vertical;

                Height = roundedHeight;
                Width = Mathf.RoundToInt(Height * aspect.Value);
            }

            public void Round(AspectRatio aspect)
            {
                var reducedRatio = aspect.GetReduced();

                var ceilDistance = reducedRatio.Vertical - Height%reducedRatio.Vertical - 1;
                var floorDistance = Height % reducedRatio.Vertical;

                if (ceilDistance >= floorDistance)
                    Floor(aspect);
                else
                    Ceil(aspect);
            }

            public bool Equals(ScreenSize other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _width == other._width && _height == other._height;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ScreenSize) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_width*397) ^ _height;
                }
            }

            public static bool operator ==(ScreenSize left, ScreenSize right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(ScreenSize left, ScreenSize right)
            {
                return !Equals(left, right);
            }
        }
    }
}
