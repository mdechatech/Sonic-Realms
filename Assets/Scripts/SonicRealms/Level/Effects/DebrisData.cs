using SonicRealms.Core.Actors;
using UnityEngine;

namespace SonicRealms.Level.Effects
{
    /// <summary>
    /// Added to objects created by CreateDebris. Contains the controller that caused CreateDebris' activation.
    /// </summary>
    [AddComponentMenu("")]
    public class DebrisData : MonoBehaviour
    {
        /// <summary>
        /// The controller that caused CreateDebris' activation.
        /// </summary>
        [Tooltip("The controller that caused CreateDebris' activation.")]
        public HedgehogController Controller;

        /// <summary>
        /// The CreateDebris component that created this debris.
        /// </summary>
        [Tooltip("The CreateDebris component that created this debris.")]
        public CreateDebris Source;

        /// <summary>
        /// The row number on which the debris was created. The bottom-most row is zero.
        /// </summary>
        [Tooltip("The row number on which the debris was created. The bottom-most row is zero.")]
        public int Row;

        /// <summary>
        /// The total number of rows of debris that were created.
        /// </summary>
        [Tooltip("The total number of rows of debris that were created.")]
        public int TotalRows;

        /// <summary>
        /// The column number on which the debris created. The left-most row is zero.
        /// </summary>
        [Tooltip("The column number on which the debris created. The left-most row is zero.")]
        public int Column;

        /// <summary>
        /// The total number of columns of debris that were created.
        /// </summary>
        [Tooltip("The total number of columns of debris that were created.")]
        public int TotalColumns;
    }
}
