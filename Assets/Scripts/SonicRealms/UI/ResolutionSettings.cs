using System;
using System.Collections.Generic;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.UI
{
    [CreateAssetMenu(fileName = "Resolution Settings", menuName = "Sonic Realms/Resolution Settings", order = -1)]
    public class ResolutionSettings : ScriptableObject
    {
        public int WindowedEntryCount { get { return _windowedEntries.Count; } }

        [SerializeField]
        private bool _enableFullscreen;

        [SerializeField]
        private List<Entry> _windowedEntries; 
        
        protected void Reset()
        {
            _enableFullscreen = true;
            _windowedEntries = new List<Entry>();
        }

        public Entry GetWindowedEntry(int index)
        {
            return _windowedEntries[index];
        }

        [Serializable]
        public class Entry
        {
            public float AspectRatio { get { return _aspectRatio.Value; } }

            public int AspectHorizontal { get { return _aspectRatio.Horizontal; } }
            public int AspectVertical { get { return _aspectRatio.Vertical; } }

            public int ScreenSizeCount { get { return _screenSizes.Count; } }

            public ScreenSize this[int index] { get { return _screenSizes[index]; } }

            [SerializeField]
            private AspectRatio _aspectRatio;

            [SerializeField]
            private List<ScreenSize> _screenSizes;

            public Entry(AspectRatio aspectRatio, int firstScreenHeight)
            {
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
        }

        [Serializable]
        public class AspectRatio
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
        }

        [Serializable]
        public class ScreenSize
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
        }
    }
}
