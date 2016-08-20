using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    /// <summary>
    /// Easy way to create fonts from a spritesheet (requires sprite in Multiple mode).
    /// </summary>
    public class SpritesheetFontCreator : EditorWindow
    {
        [MenuItem("Realms/Create Font/From Spritesheet", false, 200)]
        public static void ShowWindow()
        {
            GetWindow(typeof(SpritesheetFontCreator), false, "Create Font", true);
        }

        public Sprite SourceSprite;
        public string SourcePath;
        public Vector2 Scale;

        private void OnGUI()
        {
            Sprite[] sprites = null;

            SourceSprite = (Sprite) EditorGUILayout.ObjectField("Source", SourceSprite, typeof (Sprite), false);
            if (SourceSprite != null)
            {
                SourcePath = AssetDatabase.GetAssetPath(SourceSprite);

                GUI.enabled = false;
                EditorGUILayout.TextField("Path", SourcePath);
                GUI.enabled = true;
            }
            else
            {
                SourcePath = EditorGUILayout.TextField("Path", SourcePath);
                if (string.IsNullOrEmpty(SourcePath))
                {
                    GUI.enabled = false;
                    goto button;
                }
            }

            Scale = new Vector2(Mathf.Clamp01(Scale.x), Mathf.Clamp01(Scale.y));

            if (Scale == default(Vector2)) Scale = new Vector2(1.0f, 1.0f);

            Scale = EditorGUILayout.Vector2Field("Scale", Scale);

            EditorGUILayout.HelpBox("Name sprites to ASCII values. Last number matters only.\n\n" +
                                    "For example, the \"X\" sprite could be named \"gotta_go88mph\".", MessageType.Info);
            
            sprites = AssetDatabase.LoadAllAssetsAtPath(SourcePath).OfType<Sprite>().ToArray();

            if (sprites.Length == 0)
            {
                EditorGUILayout.HelpBox("No sprites found.", MessageType.Error);
                GUI.enabled = false;
                goto button;
            }

            SourceSprite = SourceSprite ?? sprites[0];

            button:
            if (GUILayout.Button("Create"))
            {
                Create(sprites);
            }

            GUI.enabled = true;
        }

        public void Create(Sprite[] sprites)
        {
            if (sprites == null)
                return;

            // Get last numbers in all sprite names
            var asciiStrings = sprites.Select(sprite =>
                Regex.Match(sprite.name, @"\d+", RegexOptions.RightToLeft).Value).ToArray();

            
            var success = true;
            var ascii = new int[sprites.Length];

            var commonIndex = 0;

            for (var i = 0; i < sprites.Length; ++i)
            {
                int check;
                if (int.TryParse(asciiStrings[i], out check) && check >= 0)
                {
                    // Error detection - if a name has its number in a different position compared to others,
                    // a naming error may have occurred
                    if (i == 0)
                    {
                        commonIndex = sprites[0].name.LastIndexOf(asciiStrings[i], StringComparison.Ordinal);
                    }
                    else
                    {
                        var asciiIndex = sprites[i].name.LastIndexOf(asciiStrings[i], StringComparison.Ordinal);
                        if (asciiIndex != commonIndex)
                        {
                            Debug.LogWarning(string.Format("Possible naming error: " +
                                                           "\"{0}\" vs \"{1}\".", sprites[0].name, sprites[i].name));
                        }
                    }

                    ascii[i] = check;
                    continue;
                }

                Debug.LogError(string.Format("No valid number was found for sprite \"{0}\".", sprites[i].name));
                success = false;
            }

            if (!success)
            {
                Debug.LogError("The font was not created.");
                return;
            }

            if (!ascii.Contains(32))
            {
                Debug.LogWarning("You may want to create a sprite for spaces. The ASCII value is 32.");
            }

            // Get starting directory
            var defaultPath = SourcePath;

            // Get material path
            var path = EditorUtility.SaveFilePanel("Save Material", defaultPath, 
                Path.GetFileNameWithoutExtension(SourcePath), "mat");
            if (string.IsNullOrEmpty(path))
                return;
                
            path = "Assets" + path.Substring(Mathf.Max(Application.dataPath.Length, 0));
            
            // Create material
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var exists = material != null;

            if(!exists) material = new Material(Shader.Find("UI/Default"));

            material.mainTexture = SourceSprite.texture;

            if(!exists) AssetDatabase.CreateAsset(material, path);

            // Get font path
            path = EditorUtility.SaveFilePanel("Save Font", path,
                Path.GetFileNameWithoutExtension(path), "fontsettings");
            if (string.IsNullOrEmpty(path))
                return;
            path = "Assets" + path.Substring(Application.dataPath.Length);
            
            // Create character info
            var charData = new CharacterInfo[ascii.Length];
            for (var i = 0; i < ascii.Length; ++i)
            {
                var sprite = sprites[i];
                charData[i] = FontUtility.Splice(sprites[i].texture, sprite.rect,
                    new Rect(new Vector2(), new Vector2(sprite.rect.size.x*Scale.x, sprite.rect.size.y*Scale.y)),
                    ascii[i]);
            }

            // Create font
            var font = AssetDatabase.LoadAssetAtPath<Font>(path);
            exists = font != null;

            if (!exists) font = new Font();

            font.material = material;
            font.name = Path.GetFileNameWithoutExtension(path);
            font.characterInfo = charData;

            FontUtility.SetLineSpacing(font, sprites[0].rect.height*Scale.y);

            // Save and refresh
            if(!exists) AssetDatabase.CreateAsset(font, path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
