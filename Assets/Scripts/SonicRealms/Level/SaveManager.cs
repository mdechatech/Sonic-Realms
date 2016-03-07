using System.IO;
using UnityEngine;

namespace SonicRealms.Level
{
    /// <summary>
    /// Handles saving to disk. Works for both web and standalone clients.
    /// </summary>
    public static class SaveManager
    {
        public const string FilePrefix = "";
        public const string FileSuffix = ".json";

        /// <summary>
        /// Saves the data to the location from which it was opened.
        /// </summary>
        /// <param name="saveData">The save data.</param>
        public static void Flush(this SaveData saveData)
        {
            if (saveData.Name == null) return;

            Flush(saveData, saveData.Name);
        }

        /// <summary>
        /// Saves the data with the specified file name.
        /// </summary>
        /// <param name="saveData">The save data.</param>
        /// <param name="fileName">The file name.</param>
        public static void Flush(this SaveData saveData, string fileName)
        {
            SetString(fileName, Serialize(saveData));
        }

        /// <summary>
        /// Creates the save data with the specified file name.
        /// </summary>
        /// <param name="fileName">The specified file name.</param>
        /// <returns></returns>
        public static SaveData Create(string fileName)
        {
            var data = new SaveData();
            data.Name = fileName;
            SetString(fileName, Serialize(data));
            return data;
        }

        /// <summary>
        /// Loads the save data with the specified file name.
        /// </summary>
        /// <param name="fileName">The specified file name.</param>
        /// <returns></returns>
        public static SaveData Load(string fileName)
        {
            var data = GetString(fileName);
            if (data == null)
            {
                return null;
            }
            else
            {
                var saveData = Deserialize(data);
                saveData.Name = fileName;
                return saveData;
            }
        }

        /// <summary>
        /// Creates a string from the given save data.
        /// </summary>
        /// <param name="saveData">The save data.</param>
        /// <returns></returns>
        public static string Serialize(SaveData saveData)
        {
            return JsonUtility.ToJson(saveData);
        }

        /// <summary>
        /// Creates a save data object from the given string.
        /// </summary>
        /// <param name="rawData">The given string.</param>
        /// <returns></returns>
        public static SaveData Deserialize(string rawData)
        {
            return JsonUtility.FromJson<SaveData>(rawData);
        }

        private static string GetString(string key)
        {
#if UNITY_WEBPLAYER
            return PlayerPrefs.GetString(key);
#else
            var path = ConstructPath(key);
            return File.Exists(path) ? File.ReadAllText(path) : null;
#endif
        }

        private static void SetString(string key, string value)
        {
#if UNITY_WEBPLAYER
            PlayerPrefs.SetString(key, value);
#else
            File.WriteAllText(ConstructPath(key), value);
#endif
        }

        private static string ConstructPath(string fileName)
        {
            return Application.persistentDataPath + "/" + FilePrefix + fileName + FileSuffix;
        }   
    }
}
