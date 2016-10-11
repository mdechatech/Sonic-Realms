using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// <para>Holds palette data for <see cref="PaletteCycler"/>.</para>
    /// <para>It has a primary palette line <see cref="MainLine"/> and can also partition parts of it for
    /// a greater degree of control. For more info on partitions, see <see cref="GetPartitionAt"/>.</para>
    /// </summary>
    [CreateAssetMenu(fileName = "Palette Line", menuName = "Sonic Realms/Palette Line")]
    public class PaletteLine : ScriptableObject
    {
        #region Public Fields & Properties

        public const int DefaultMaxColors = 16;

        /// <summary>
        /// For internal editor use.
        /// </summary>
        public bool IsDirty { get { return _prevMaxColors != _maxColors; } }

        /// <summary>
        /// Number of colors each entry in the palette line holds.
        /// </summary>
        public int MaxColors { get { return _maxColors; } }

        /// <summary>
        /// The main palette line. Each entry holds as many colors as specified by <see cref="MaxColors"/>.
        /// </summary>
        public Partition MainLine { get { return _mainLine; } set { _mainLine = value; } }

        /// <summary>
        /// Number of partitions on the main palette line.
        /// </summary>
        public int PartitionCount { get { return _partitions.Count; } }

        #endregion

        #region Private & Inspector Fields

        [SerializeField, HideInInspector]
        private int _prevMaxColors;

        [SerializeField]
        [Range(1, DefaultMaxColors)]
        private int _maxColors;

        [SerializeField]
        [Foldout("Main Line")]
        private Partition _mainLine;

        [SerializeField]
        [Foldout("Partitions")]
        private List<Partition> _partitions;

        #endregion

        #region Public & Helper Functions

        /// <summary>
        /// Clears the main palette line and all partitioned palette lines.
        /// </summary>
        public void ClearPalette()
        {
            _mainLine = new Partition(_maxColors);
            _partitions.Clear();
        }

        /// <summary>
        /// Resizes the main palette line to hold the given number of colors. WATCH OUT! Passing in a
        /// lower number from the current <see cref="MaxColors"/> results in the extra colors getting
        /// trashed.
        /// </summary>
        public void ResizePalette(int count)
        {
            _prevMaxColors = _maxColors = count;

            MainLine.ResizeEntries(count);
        }

        /// <summary>
        /// <para>
        /// Returns the partition at the given index. Use <see cref="PartitionCount"/> for bounds-checking.
        /// </para>
        /// <para>
        /// A partition is an extra palette line that takes up parts of the <see cref="MainLine"/>. It can
        /// be cycled independently of the main palette line and has its own separate entries.
        /// </para>
        /// </summary>
        public Partition GetPartitionAt(int index)
        {
            return _partitions[index];
        }

        /// <summary>
        /// Returns the partition that is taking up the given color index. For more info on partitions, see
        /// <see cref="GetPartitionAt"/>.
        /// <returns>The index of the partition, or -1 if the <see cref="MainLine"/> is using the color index.</returns>
        /// </summary>
        public int GetIndexOfPartitionFor(int colorIndex)
        {
            for (var i = 0; i < _partitions.Count; ++i)
            {
                var partition = _partitions[i];

                if (partition.HasIndex(colorIndex))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns the partition that is taking up the given color index. For more info on partitions, see
        /// <see cref="GetPartitionAt"/>.
        /// <returns>The partition, <see cref="MainLine"/> is using the color index.</returns>
        /// </summary>
        public Partition GetPartitionFor(int colorIndex)
        {
            for (var i = 0; i < _partitions.Count; ++i)
            {
                var partition = _partitions[i];

                if (partition.HasIndex(colorIndex))
                    return partition;
            }

            return null;
        }

        /// <summary>
        /// Creates a blank partition from the <see cref="MainLine"/> that takes up the given color indices.
        /// For more info on partitions, see <see cref="GetPartitionAt"/>.
        /// </summary>
        public void AddBlankPartition(HashSet<int> colorIndices)
        {
            _partitions.Add(new Partition(colorIndices));
        }

        /// <summary>
        /// Inserts a blank partition from the <see cref="MainLine"/> that takes up the given color indices.
        /// For more info on partitions, see <see cref="GetPartitionAt"/>.
        /// </summary>
        public void InsertBlankPartition(int index, HashSet<int> colorIndices)
        {
            _partitions.Insert(index, new Partition(colorIndices));
        }

        /// <summary>
        /// Frees up the partition at the given index. Use <see cref="PartitionCount"/> for bounds checking.
        /// For more info on partitions, see <see cref="GetPartitionAt"/>.
        /// </summary>
        public void RemovePartitionAt(int index)
        {
            _partitions.RemoveAt(index);
        }

        #endregion

        #region Lifecycle Functions

        protected void Reset()
        {
            _prevMaxColors = _maxColors = 16;
            _mainLine = new Partition(_maxColors);
        }

        #endregion

        #region Nested Classes

        [Serializable]
        public class Partition
        {
            [SerializeField]
            private List<int> _indices;

            [SerializeField]
            private List<ColorList> _entries;

            [SerializeField]
            private int _listLength;

            public int ListLength { get { return _listLength; } }

            public int EntryCount { get { return _entries.Count; } }

            public Partition(int listLength) : this(new HashSet<int>(Enumerable.Range(0, listLength)))
            {

            }

            public Partition(HashSet<int> indices)
            {
                _listLength = indices.Count;
                _indices = indices.ToList();
                _entries = new List<ColorList>();
            }

            public bool HasIndex(int index)
            {
                return _indices.Contains(index);
            }

            public HashSet<int> GetIndices()
            {
                return new HashSet<int>(_indices);
            }

            public void AddBlankEntries()
            {
                _entries.Add(new ColorList(_listLength));
            }

            public void CopyAppendLastEntries()
            {
                var list = new ColorList(_listLength);

                if (_entries.Count > 0)
                {
                    var lastList = _entries[_entries.Count - 1];

                    for (var i = 0; i < _listLength; ++i)
                    {
                        list[i] = lastList[i];
                    }
                }

                _entries.Add(list);
            }

            public void InsertBlankEntries(int index)
            {
                _entries.Insert(index, new ColorList(_listLength));
            }

            public ColorList GetEntriesAt(int index)
            {
                return _entries[index];
            }

            public void RemoveEntriesAt(int index)
            {
                _entries.RemoveAt(index);
            }

            public void ClearEntries()
            {
                _entries.Clear();
            }

            public void ResizeEntries(int count)
            {
                _listLength = count;

                foreach (var entry in _entries)
                {
                    entry.Resize(count);
                }
            }

            [Serializable]
            public class ColorList
            {
                public int Count { get { return _list.Length; } }

                public Color this[int index] { get { return _list[index]; } set { _list[index] = value; } }

                [SerializeField]
                private Color[] _list;

                public ColorList(int count)
                {
                    _list = new Color[count];
                }

                public void Clear()
                {
                    for (var i = 0; i < _list.Length; ++i)
                        _list[i] = new Color();
                }

                public void Resize(int count)
                {
                    Array.Resize(ref _list, count);
                }

                public void CycleForward()
                {
                    var last = _list[_list.Length - 1];

                    for (var i = _list.Length - 2; i >= 0; --i)
                    {
                        _list[i + 1] = _list[i];
                    }

                    _list[0] = last;
                }

                public void CycleBackward()
                {
                    var first = _list[0];

                    for (var i = 1; i < _list.Length; ++i)
                    {
                        _list[i - 1] = _list[i];
                    }

                    _list[_list.Length - 1] = first;
                }
            }
        }

        #endregion
    }
}
