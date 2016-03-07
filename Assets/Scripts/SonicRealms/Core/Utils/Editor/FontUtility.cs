using UnityEditor;
using UnityEngine;

namespace SonicRealms.Core.Utils.Editor
{
    /// <summary>
    /// Helpers used for font creation.
    /// </summary>
    public static class FontUtility
    {
        /// <summary>
        /// Creates a material from the specified texture using the default UI shader.
        /// </summary>
        /// <param name="texture2D">The specified texture.</param>
        /// <returns></returns>
        public static Material Texture2DToMaterial(Texture2D texture2D)
        {
            return new Material(Shader.Find("UI/Default")) { mainTexture = texture2D };
        }

        /// <summary>
        /// Creates the data for a character using pixel rects.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="sourceRect">The area on the source texture to use.</param>
        /// <param name="displayRect">The dimensions of the character on screen. Height extends the character
        /// upward and must be positive.</param>
        /// <param name="index">The character code associated with the character.</param>
        /// <returns></returns>
        public static CharacterInfo Splice(Texture2D source, Rect sourceRect, Rect displayRect, int index = 0)
        {
            return new CharacterInfo()
            {
                index = index,
                advance = (int) displayRect.width,

                uvTopLeft = new Vector2(sourceRect.min.x/source.width, sourceRect.max.y/source.height),
                uvTopRight = new Vector2(sourceRect.max.x/source.width, sourceRect.max.y/source.height),
                uvBottomLeft = new Vector2(sourceRect.min.x/source.width, sourceRect.min.y/source.height),
                uvBottomRight = new Vector2(sourceRect.max.x/source.width, sourceRect.min.y/source.height),

                minX = (int) displayRect.min.x,
                maxX = (int) displayRect.max.x,
                minY = -(int) displayRect.max.y,
                maxY = (int) displayRect.min.y,
            };
        }

        /// <summary>
        /// Sets the line spacing for the specified font.
        /// </summary>
        /// <param name="font">The specified font.</param>
        /// <param name="value">The specified value, measured in pixels.</param>
        public static void SetLineSpacing(Font font, float value)
        {
            SerializedObject serialized = new SerializedObject(font);
            serialized.FindProperty("m_LineSpacing").floatValue = value;
            serialized.ApplyModifiedProperties();
        }
    }
}
