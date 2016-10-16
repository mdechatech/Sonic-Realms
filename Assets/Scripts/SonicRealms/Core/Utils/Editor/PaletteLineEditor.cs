using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    /// <summary>
    /// BUG The editor ballses up when Reset is called
    /// </summary>
    [CustomEditor(typeof(PaletteLine))]
    public class PaletteLineEditor : BaseRealmsEditor
    {
        private PaletteLine _instance;

        private ReorderableList _mainLineRList;
        private List<ReorderableList> _partitionRLists;

        private bool[] _indexButtonStates;

        protected bool ShowCreatePartitionFoldout
        {
            get { return EditorPrefs.GetBool("PaletteLineEditor.ShowCreatePartitionFoldout", false); }
            set { EditorPrefs.SetBool("PaletteLineEditor.ShowCreatePartitionFoldout", value); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _instance = (PaletteLine) target;

            InitializeMainLineRList();
            InitializePartitionRLists();

            _indexButtonStates = new bool[_instance.MaxColors];

            serializedObject.Update();
        }

        protected override void DrawProperty(PropertyData property, GUIContent label)
        {
            if (property.Name == "_maxColors")
            {
                base.DrawProperty(property, label);

                if (GUILayout.Button("Resize Palette"))
                {
                    if (EditorUtility.DisplayDialog("Resize Palette?",
                        "Shrinking the palette size will result in loss of " +
                        "data. Are you sure you want to resize the palette?",
                        "Yes", "No"))
                    {
                        Undo.RecordObject(_instance, "Resize Palette");
                        _instance.ResizePalette(_instance.MaxColors);
                        OnEnable();
                    }
                }
                else if (_instance.IsDirty)
                {
                    EditorGUILayout.HelpBox("Max Colors has changed. The palette must be resized before any " +
                                            "further changes can be made.", MessageType.Info);
                }
            }
            else if (_instance.IsDirty)
            {
                return;
            }
            else if (property.Name == "_mainLine")
            {
                if (_instance.MainLine.EntryCount != _mainLineRList.count)
                    OnEnable();

                _mainLineRList.DoLayoutList();
            }
            else if (property.Name == "_partitions")
            {
                if (_partitionRLists.Count != _instance.PartitionCount)
                    OnEnable();

                for(var i = 0; i < _partitionRLists.Count; ++i)
                {
                    if (GUILayout.Button("Delete Partition " + i))
                    {
                        Undo.RecordObject(_instance, "Delete Partition");

                        _instance.RemovePartitionAt(i);
                        serializedObject.Update();
                        OnEnable();

                        return;
                    }

                    _partitionRLists[i].DoLayoutList();
                }
            }
            else
            {
                base.DrawProperty(property, label);
            }
        }

        protected override bool DrawFoldout(bool state, FoldoutData foldout)
        {
            if (_instance.IsDirty)
                return state;

            if (foldout.Name == "Partitions")
            {
                if (ShowCreatePartitionFoldout = EditorGUILayout.Foldout(ShowCreatePartitionFoldout, "Create Partition"))
                {
                    DrawCreatePartition();
                }

                return base.DrawFoldout(state, foldout);
            }
            else
            {
                return base.DrawFoldout(state, foldout);
            }
        }

        private void DrawCreatePartition()
        {
            EditorGUILayout.LabelField("Select Indices");

            GUILayout.BeginHorizontal();
            
            if (_indexButtonStates.Length != _instance.MaxColors)
                Array.Resize(ref _indexButtonStates, _instance.MaxColors);

            for (var i = 0; i < _instance.MaxColors; ++i)
            {
                /*
                var partitionIndex = _instance.GetIndexOfPartitionFor(i);
                if (partitionIndex >= 0)
                {
                    GUI.enabled = false;

                    _indexButtonStates[i] = false;

                    GUILayout.Button(string.Format("{0}({1})", i, partitionIndex),
                        new GUIStyle(EditorStyles.toolbarButton));

                    GUI.enabled = true;
                }
                else
                {
                */
                var indexState = _indexButtonStates[i];
                if (!indexState)
                {
                    if (GUILayout.Button(i.ToString(), new GUIStyle(EditorStyles.toolbarButton)))
                    {
                        _indexButtonStates[i] = true;
                    }
                }
                else
                {
                    if (GUILayout.Button(i.ToString(),
                        new GUIStyle(EditorStyles.toolbarButton) { normal = EditorStyles.toolbarButton.active }))
                    {
                        _indexButtonStates[i] = false;
                    }
                }
            }

            GUILayout.EndHorizontal();
            
            var indices = new HashSet<int>();

            for (var i = 0; i < _indexButtonStates.Length; ++i)
            {
                if (_indexButtonStates[i])
                    indices.Add(i);
            }

            GUI.enabled = indices.Count > 0;

            if (GUILayout.Button("Create"))
            {
                Undo.RecordObject(_instance, "Create Palette Partition");

                _instance.AddBlankPartition(indices);
                serializedObject.Update();

                for (var i = 0; i < _indexButtonStates.Length; ++i)
                    _indexButtonStates[i] = false;

                InitializePartitionRLists();
            }

            GUI.enabled = true;
        }

        private void InitializeMainLineRList()
        {
            var mainLineProp = serializedObject.FindProperty("_mainLine");

            _mainLineRList = CreatePartitionRList(mainLineProp, _instance.MainLine, "Main Line");
        }

        private void InitializePartitionRLists()
        {
            _partitionRLists = new List<ReorderableList>();

            var partitionsProp = serializedObject.FindProperty("_partitions");

            for (var i = 0; i < _instance.PartitionCount; ++i)
            {
                var partitionProp = partitionsProp.GetArrayElementAtIndex(i);

                _partitionRLists.Add(CreatePartitionRList(partitionProp, _instance.GetPartitionAt(i),
                    "Partition " + i));
            }
        }

        private ReorderableList CreatePartitionRList(SerializedProperty partitionProp, PaletteLine.Partition partition,
            string name)
        {
            var entriesProp = partitionProp.FindPropertyRelative("_entries");

            var rlist = new ReorderableList(
                serializedObject,
                entriesProp);

            // Color dimensions
            const int colorLabelWidth = 20;
            const int colorWidth = 34;
            const int colorHeight = 17;
            const int colorsPerRow = 8;

            rlist.elementHeight = Mathf.Max(colorHeight * 2,
                colorHeight * Mathf.CeilToInt(_instance.MaxColors / (float)colorsPerRow));

            // Header
            rlist.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, name);
            };

            // Add
            rlist.onAddCallback = list =>
            {
                Undo.RecordObject(_instance, "Add Palette Line Entry");
                partition.CopyAppendLastEntries();
            };

            rlist.onCanRemoveCallback = list =>
            {
                return list.count > 1;
            };

            // Remove
            rlist.onRemoveCallback = list =>
            {
                Undo.RecordObject(_instance, string.Format("Remove Partition Entry"));
                partition.RemoveEntriesAt(list.index);
            };

            // Draw
            rlist.drawElementCallback = (rect, index, active, focused) =>
            {
                var entries = partition.GetEntriesAt(index);

                var curX = rect.x;
                var curY = rect.y;

                // Label
                const int labelWidth = 17;

                EditorGUI.LabelField(new Rect(curX, curY, labelWidth, rect.height),
                    index.ToString(),
                    new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft });

                curX += labelWidth;

                // Buttons
                const int ButtonsWidth = 34;
                const int ButtonsPad = 5;

                if (GUI.Button(new Rect(curX, curY, ButtonsWidth, (rect.height - ButtonsPad) * 0.5f),
                    "->"))
                {
                    Undo.RecordObject(_instance, "Cycle Colors Forward");
                    entries.CycleForward();
                }
                else if (
                    GUI.Button(
                        new Rect(curX, curY + (rect.height - ButtonsPad) * 0.5f, ButtonsWidth,
                            (rect.height - ButtonsPad) * 0.5f),
                        "<-"))
                {
                    Undo.RecordObject(_instance, "Cycle Colors Backward");
                    entries.CycleBackward();
                }

                curX += ButtonsWidth + 5;

                var i = 0;
                foreach (var colorIndex in partition.GetIndices())
                {
                    if (colorIndex >= _instance.MaxColors)
                        continue;

                    var entry = entries[i];
                    
                    // Color label
                    EditorGUI.LabelField(
                        new Rect(curX, curY, colorLabelWidth, colorHeight),
                        colorIndex.ToString());

                    curX += colorLabelWidth;

                    // Color field
                    var color = EditorGUI.ColorField(new Rect(curX, curY, colorWidth, colorHeight),
                        entry);

                    if (color != entries[i])
                    {
                        Undo.RecordObject(_instance, "Set Partition Entry Color");
                        entries[i] = color;
                    }


                    curX += colorWidth;

                    if ((i + 1) % colorsPerRow == 0)
                    {
                        curX -= (colorLabelWidth + colorWidth) * colorsPerRow;
                        curY += colorHeight;
                    }

                    ++i;
                }
            };

            return rlist;
        }
    }
}
