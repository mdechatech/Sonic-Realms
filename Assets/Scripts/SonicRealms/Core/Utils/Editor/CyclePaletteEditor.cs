using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    // TODO document this behemoth
    [CustomEditor(typeof(CyclePalette))]
    public class CyclePaletteEditor : UnityEditor.Editor
    {
        protected bool ShowPalettes
        {
            get { return EditorPrefs.GetBool("CyclePaletteEditor.ShowPalettes", false); }
            set { EditorPrefs.SetBool("CyclePaletteEditor.ShowPalettes", value); }
        }

        private const int ColorsPerPalette = CyclePalette.ColorsPerPalette;
        private const int ColorsPerRow = 8;
        private const int Rows = ColorsPerPalette/ColorsPerRow;

        private const int LabelOffset = 17;
        private const int ButtonOffset = 34;
        private const int Spacing = 5;

        private CyclePalette Instance;

        private ReorderableList Palettes;
        private List<int> Divisions;

        public void OnEnable()
        {
            Instance = target as CyclePalette;
            InitializeList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HedgehogEditorGUIUtility.DrawProperties(serializedObject,
                "PaletteMaterial", "IgnoreTransparent");

            Instance.SetColorFrom = EditorGUILayout.Toggle("Set Color From", Instance.SetColorFrom);
            if (Instance.SetColorFrom)
            {
                HedgehogEditorGUIUtility.DrawProperties(serializedObject, "ColorFromIndex");
            }

            ShowPalettes = EditorGUILayout.Foldout(ShowPalettes, "Palettes");
            if (ShowPalettes)
            {
                Palettes.DoLayoutList();
                if(!Application.isPlaying && Palettes.HasKeyboardControl())
                    Instance.SetPalette(Palettes.index);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeList()
        {
            Divisions = new List<int>();
            Divisions.AddRange(Enumerable.Range(0, Instance.AllColors.Count/ColorsPerPalette));

            Palettes = new ReorderableList(Divisions, typeof(Color));
            Palettes.elementHeight = 17*Mathf.Max(Rows, 2);

            Palettes.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Palettes");
            };

            Palettes.onAddCallback = list =>
            {
                if (!Divisions.Any())
                    Instance.AllColors.AddRange(Enumerable.Repeat(new Color(), ColorsPerPalette));
                else
                    Instance.AllColors.AddRange(new List<Color>(Instance.AllColors.GetRange(
                        (Divisions.Count - 1) * ColorsPerPalette, ColorsPerPalette)));

                Divisions.Add(Divisions.Count);
            };

            Palettes.onRemoveCallback = list =>
            {
                Divisions.RemoveAt(list.index);
                Instance.AllColors.RemoveRange(list.index*ColorsPerPalette, ColorsPerPalette);

                for (var i = list.index; i < Divisions.Count; ++i)
                    Divisions[i] = i;
            };

            Palettes.onReorderCallback = list =>
            {
                var palettes = new List<Color>(Instance.AllColors);
                Instance.AllColors = new List<Color>();
                for (var i = 0; i < Divisions.Count; ++i)
                {
                    for (var j = 0; j < ColorsPerPalette; ++j)
                        Instance.AllColors.Add(palettes[j + ColorsPerPalette*Divisions[i]]);

                    Divisions[i] = i;
                }
            };

            Palettes.onCanRemoveCallback = list => Divisions.Count > 0;

            Palettes.drawElementCallback = (rect, index, active, focused) =>
            {
                if (GUI.Button(new Rect(rect.x + LabelOffset, rect.y, ButtonOffset, (rect.height - Spacing)/2), "->"))
                {
                    var start = index*ColorsPerPalette;
                    var end = index*ColorsPerPalette + ColorsPerPalette - 1;

                    var last = Instance.IgnoreTransparent
                        ? Instance.AllColors.GetRange(start, ColorsPerPalette).Last(color => color != Color.clear)
                        : Instance.AllColors[start];

                    var first = -1;
                    for (var i = start; i <= end; ++i)
                    {
                        if (Instance.IgnoreTransparent && Instance.AllColors[i].a == 0.0f)
                            continue;

                        if (first == -1)
                            first = i;

                        var swap = Instance.AllColors[i];
                        Instance.AllColors[i] = last;
                        last = swap;
                    }

                    if(first >= 0) Instance.AllColors[first] = last;
                }

                if (
                    GUI.Button(
                        new Rect(rect.x + LabelOffset, rect.y + (rect.height - Spacing)/2, ButtonOffset,
                            (rect.height - Spacing)/2), "<-"))
                {
                    var start = index*ColorsPerPalette;
                    var end = index*ColorsPerPalette + ColorsPerPalette - 1;

                    var first = Instance.IgnoreTransparent
                        ? Instance.AllColors.GetRange(start, ColorsPerPalette).First(color => color != Color.clear)
                        : Instance.AllColors[start];

                    var last = -1;
                    for (var i = end; i >= start; --i)
                    {
                        if (Instance.IgnoreTransparent && Instance.AllColors[i].a == 0.0f)
                            continue;

                        if (last == -1)
                            last = i;

                        var swap = Instance.AllColors[i];
                        Instance.AllColors[i] = first;
                        first = swap;
                    }

                    if (last >= 0) Instance.AllColors[last] = first;
                }

                var dx = (rect.width - LabelOffset - ButtonOffset)/ColorsPerRow;
                var dy = (rect.height - Spacing)/Rows;

                EditorGUI.LabelField(new Rect(rect.x, .75f*rect.min.y + .25f*rect.max.y - Spacing/2.0f, LabelOffset, LabelOffset),
                    index.ToString(),
                    EditorStyles.boldLabel);

                EditorGUI.DrawRect(new Rect(rect.min.x, rect.min.y, rect.width, Spacing), Color.clear);

                for (var i = index * ColorsPerPalette; i < index * ColorsPerPalette + ColorsPerPalette; ++i)
                {
                    var relative = i - index*ColorsPerPalette;

                    var rectX = rect.min.x + dx*(relative%ColorsPerRow) + LabelOffset + ButtonOffset;
                    var rectY = rect.min.y + dy*(relative/ColorsPerRow);

                    var color = Instance.AllColors[i];

                    var selectedIgnore = new Color(0.541f, 0.651f, 0.808f);
                    var normalIgnore = Color.gray;
                    var normal = Color.black;

                    EditorGUI.LabelField(new Rect(rectX, rectY, dx/3, dy), (relative + 1).ToString("D2"),
                        Instance.IgnoreTransparent && color.a == 0.0f
                            ? Palettes.index == index
                                ? new GUIStyle(EditorStyles.label)
                                {
                                    normal = new GUIStyleState() {textColor = selectedIgnore}
                                }
                                : new GUIStyle(EditorStyles.label)
                                {
                                    normal = new GUIStyleState() {textColor = normalIgnore}
                                }

                            : new GUIStyle(EditorStyles.label) {normal = new GUIStyleState() {textColor = normal}});

                    Instance.AllColors[i] = EditorGUI.ColorField(new Rect(rectX + dx/3, rectY, 2*dx/3, dy),
                        color);

                    var newColor = Instance.AllColors[i];
                    if(color != newColor && Mathf.Abs(color.a - newColor.a) < 0.001f)
                        Instance.AllColors[i] = new Color(newColor.r, newColor.g, newColor.b, 1.0f);
                }
            };
        }
    }
}
