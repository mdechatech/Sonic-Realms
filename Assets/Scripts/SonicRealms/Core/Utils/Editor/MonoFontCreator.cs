using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    /// <summary>
    /// Easy way to create monospaced fonts from a fixed-width spritesheet.
    /// </summary>
    public class MonoFontCreator : EditorWindow
    {
        [MenuItem("Hedgehog/Create Font/Monospaced")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MonoFontCreator), false, "Create Font", true);
        }

        public Texture2D Source;

        public int CharactersPerRow;
        public int CharactersPerColumn;

        public int CharacterPixelWidth;
        public int CharacterPixelHeight;

        public IList AsciiMappings;
        protected ReorderableList AsciiValuesList;

        private Vector2 _scrollPosition;

        public void Init()
        {
            Source = null;

            CharactersPerRow = 10;
            CharactersPerColumn = 10;

            CharacterPixelWidth = 10;
            CharacterPixelHeight = 12;

            AsciiMappings = new List<int>();

            AsciiValuesList = new ReorderableList(AsciiMappings, typeof (int))
            {
                onAddCallback = list => AsciiMappings.Add(AsciiMappings.Count == 0
                    ? 48
                    : (int) AsciiMappings[AsciiMappings.Count - 1] + 1),
                onCanRemoveCallback = list => AsciiMappings.Count > 0,

                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Spritesheet Position -> ASCII Value"),
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.LabelField(rect,
                        string.Format("col {0}, row {1}: ", index%CharactersPerColumn, index/CharactersPerColumn));

                    AsciiMappings[index] = EditorGUI.IntField(
                        new Rect(rect.x + rect.width/2.0f, rect.y, rect.width/2.0f, rect.height),
                        (int) AsciiMappings[index]);
                }
            };
        }

        private void OnGUI()
        {
            if (AsciiMappings == null)
            {
                Init();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Bitmap Font Creator", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            Source = (Texture2D) EditorGUILayout.ObjectField("Source", Source, typeof (Texture2D), false);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Slicing", EditorStyles.boldLabel);
            CharactersPerColumn = Mathf.Clamp(EditorGUILayout.IntField("Characters Per Column", CharactersPerColumn), 1, 100);
            CharactersPerRow = Mathf.Clamp(EditorGUILayout.IntField("Characters Per Row", CharactersPerRow), 1, 100);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Character Size", EditorStyles.boldLabel);
            CharacterPixelWidth = Mathf.Clamp(EditorGUILayout.IntField("Character Pixel Width", CharacterPixelWidth), 1, int.MaxValue);
            CharacterPixelHeight = Mathf.Clamp(EditorGUILayout.IntField("Character Pixel Height", CharacterPixelHeight), 1, int.MaxValue);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("ASCII Mappings", EditorStyles.boldLabel);
            AsciiValuesList.DoLayoutList();

            EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
                Create();

            EditorGUILayout.EndScrollView();
        }

        public void Create()
        {
            // Error handling
            if (Source == null)
            {
                Debug.LogError("Please specify a Source Texture2D.");
                return;
            }

            if (AsciiMappings.Count < 1)
            {
                Debug.LogError("Please map at least one glyph to an ASCII value.");
                return;
            }

            // Get starting directory
            var defaultPath = AssetDatabase.GetAssetPath(Source);

            // Get material path
            var path = EditorUtility.SaveFilePanel("Save Material", defaultPath, Source.name, "mat");
            if (string.IsNullOrEmpty(path))
                return;

            path = "Assets" + path.Substring(Mathf.Clamp(Application.dataPath.Length, 0, int.MaxValue));

            // Create material
            var material = FontUtility.Texture2DToMaterial(Source);

            // Save material
            AssetDatabase.CreateAsset(material, path);

            // Get font path
            path = EditorUtility.SaveFilePanel("Save Font", path, Source.name, "fontsettings");
            if (string.IsNullOrEmpty(path))
                return;

            path = "Assets" + path.Substring(Application.dataPath.Length);

            // Get character count
            var charCount = Mathf.Min(AsciiMappings.Count, CharactersPerRow*CharactersPerColumn);
            var charData = new CharacterInfo[charCount];

            // Create character info
            for (var i = 0; i < charCount; ++i)
            {
                var col = i%CharactersPerColumn;
                var row = i/CharactersPerColumn;

                charData[i] = new CharacterInfo()
                {
                    index = (int) AsciiMappings[i],
                    advance = CharacterPixelWidth,

                    uvTopLeft = new Vector2(col/(float)CharactersPerRow, 1.0f - row/(float)CharactersPerColumn),
                    uvTopRight = new Vector2((col + 1)/(float)CharactersPerRow, 1.0f - row/(float)CharactersPerColumn),
                    uvBottomLeft = new Vector2(col/(float)CharactersPerRow, 1.0f - (row + 1)/(float)CharactersPerColumn),
                    uvBottomRight = new Vector2((col + 1)/(float)CharactersPerRow, 1.0f - (row + 1)/(float)CharactersPerColumn),

                    minX = 0,
                    maxX = CharacterPixelWidth,
                    minY = -CharacterPixelHeight,
                    maxY = 0,
                };
            }

            // Create font
            var font = new Font()
            {
                material = material,
                name = Path.GetFileNameWithoutExtension(path),
                characterInfo = charData,
            };

            // Save and refresh
            AssetDatabase.CreateAsset(font, path);
            AssetDatabase.Refresh();
        }
    }
}
