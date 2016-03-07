using System;
using System.Linq;
using UnityEngine;

namespace SonicRealms.Level
{
    /// <summary>
    /// Determines what the next level should be. Able to make special cases for different characters.
    /// </summary>
    [Serializable]
    public class NextLevelData
    {
        /// <summary>
        /// If there are no special cases for the current character, this will be the next level.
        /// </summary>
        [Tooltip("If there is no special cases for the current character, this will be the next level.")]
        public LevelData DefaultLevel;

        /// <summary>
        /// A list of special cases assigning different levels to different characters.
        /// </summary>
        [Tooltip("A list of special cases assigning different levels to different characters.")]
        public NextLevelCharacterOverride[] SpecialCharacters;

        public LevelData GetNextLevelFor(CharacterData character)
        {
            var special = SpecialCharacters.FirstOrDefault(data => data.Character == character);
            if (special != null) return special.NextLevel;

            return DefaultLevel;
        }
    }
}
