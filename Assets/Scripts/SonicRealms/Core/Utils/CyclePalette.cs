using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Implements palette cycling using the Palette Controller shader and the Paletted Sprite material.
    /// </summary>
    public class CyclePalette : MonoBehaviour
    {
        /// <summary>
        /// Colors in a palette. 
        /// 
        /// WARNING: if you change this, you must also add the appropriate variables in the shader.
        /// Also, the palettes you currently have will get messed up... gonna fix this later.
        /// </summary>
        public const int ColorsPerPalette = 8;

        /// <summary>
        /// The base name of the material's Color From property.
        /// </summary>
        private const string ColorFrom = "_ColorFrom";

        /// <summary>
        /// The base name of the material's Color To property.
        /// </summary>
        private const string ColorTo = "_ColorTo";

        /// <summary>
        /// A list of ids for each Color To property, one for each palette color.
        /// </summary>
        private int[] ColorToIDs;

        /// <summary>
        /// Whether to initialize the material's palette with one of these palettes.
        /// </summary>
        [Tooltip("Whether to initialize the material's palette with one of these palettes.")]
        public bool SetColorFrom;

        /// <summary>
        /// The index of the palette to initialize to. Colors on the original image must match these,
        /// or there will be no effect.
        /// </summary>
        [Tooltip("The index of the palette to initialize to. Colors on the original image must match these, " +
                 "or there will be no effect.")]
        public int ColorFromIndex;

        /// <summary>
        /// The target material to modify.
        /// </summary>
        [Tooltip("The target material to modify.")]
        public Material PaletteMaterial;

        /// <summary>
        /// Whether to ignore transparent colors on the palette when cycling it.
        /// </summary>
        [Tooltip("Whether to ignore transparent colors on the palette when cycling it.")]
        public bool IgnoreTransparent;

        /// <summary>
        /// A list of colors, virtually grouped by their palette index. For example, if there are 16 colors in a palette,
        /// the second palette starts from the 16th color in the list.
        /// </summary>
        [FormerlySerializedAs("Palettes")]
        public List<Color> AllColors;

        /// <summary>
        /// The index of the palette currently being used.
        /// </summary>
        [Tooltip("The index of the palette currently being used.")]
        public int CurrentIndex;

        public UnityEvent OnPaletteChange;

        public IEnumerable<Color> this[int index]
        {
            get { return AllColors.GetRange(index*ColorsPerPalette, ColorsPerPalette); }
            set
            {
                var offset = index*ColorsPerPalette;
                foreach (var color in value)
                    AllColors[offset++] = color;
            }
        }

        public int PaletteCount
        {
            get { return AllColors.Count/ColorsPerPalette; }
        }

        public void Reset()
        {
            PaletteMaterial = GetComponent<SpriteRenderer>() ? GetComponent<SpriteRenderer>().sharedMaterial : null;

            SetColorFrom = true;
            ColorFromIndex = 0;

            IgnoreTransparent = true;

            AllColors = new List<Color>();
            OnPaletteChange = new UnityEvent();
        }

        public void Awake()
        {
            CurrentIndex = 0;
            OnPaletteChange = OnPaletteChange ?? new UnityEvent();
        }

        public void Start()
        {
            // Set color from
            if (SetColorFrom)
            {
                for (var i = 0; i < ColorsPerPalette; ++i)
                {
                    var absolute = i + ColorFromIndex*ColorsPerPalette;
                    if (!IgnoreTransparent || AllColors[absolute].a != 0.0f)
                        PaletteMaterial.SetColor(ColorFrom + (i + 1), AllColors[absolute]);
                }
            }

            // Cache our Color To IDs
            ColorToIDs = new int[ColorsPerPalette];
            for (var i = 0; i < ColorsPerPalette; ++i)
                ColorToIDs[i] = Shader.PropertyToID(ColorTo + (i + 1));
        }

        /// <summary>
        /// Sets the palette to the specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        public void SetPalette(int index)
        {
            // If the number is out of bounds just mod it
            index = DMath.Modp(index, PaletteCount);
            
            for (var i = 0; i < ColorsPerPalette; ++i)
            {
                var absolute = i + index*ColorsPerPalette;
                if (!IgnoreTransparent || AllColors[absolute].a != 0.0f)
                {
#if UNITY_EDITOR
                    PaletteMaterial.SetColor(ColorTo + (i + 1), AllColors[absolute]);
#else
                    PaletteMaterial.SetColor(ColorToIDs[i], AllColors[absolute]);
#endif
                }
                    
            }

            CurrentIndex = index;
            OnPaletteChange.Invoke();
        }

        /// <summary>
        /// Sets the palette to the next index. If the index is out of bounds, the palette loops back
        /// to the first in the list.
        /// </summary>
        public void NextPalette()
        {
            SetPalette(CurrentIndex + 1);
        }

        /// <summary>
        /// Sets the palette to the previous index. If the index is out of bounds, the palette loops forward
        /// to the last in the list.
        /// </summary>
        public void PreviousPalette()
        {
            SetPalette(CurrentIndex - 1);
        }
    }
}
