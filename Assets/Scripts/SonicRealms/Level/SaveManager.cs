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
        public const string FileSuffix = ".save";

        public const string GlobalFileName = "globalsave";

        /// <summary>
        /// Saves the data to the location from which it was opened.
        /// </summary>
        /// <param name="saveData">The save data.</param>
        public static void Flush(this SaveData saveData)
        {
            if (saveData.Name != null)
                Flush(saveData, saveData.Name);
        }

        public static void Flush(this GlobalSaveData globalSaveData)
        {
            SetGlobalString(Serialize(globalSaveData));;
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

        public static GlobalSaveData CreateGlobalSave()
        {
            var data = new GlobalSaveData();
            data.Flush();

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
                var saveData = DeserializeSave(data);
                saveData.Name = fileName;
                return saveData;
            }
        }

        public static GlobalSaveData LoadGlobalSave()
        {
            var data = GetGlobalString();
            if (data == null)
                return null;

            return DeserializeGlobalSave(data);
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
        /// Creates a string from the given save data.
        /// </summary>
        /// <param name="globalSaveData">The save data.</param>
        /// <returns></returns>
        public static string Serialize(GlobalSaveData globalSaveData)
        {
            return JsonUtility.ToJson(globalSaveData);
        }

        /// <summary>
        /// Creates a save data object from the given string.
        /// </summary>
        /// <param name="rawData">The given string.</param>
        /// <returns></returns>
        public static SaveData DeserializeSave(string rawData)
        {
            return JsonUtility.FromJson<SaveData>(rawData);
        }

        /// <summary>
        /// Creates a save data object from the given string.
        /// </summary>
        /// <param name="rawData">The given string.</param>
        /// <returns></returns>
        public static GlobalSaveData DeserializeGlobalSave(string rawData)
        {
            return JsonUtility.FromJson<GlobalSaveData>(rawData);
        }

        private static string GetString(string key)
        {
            var path = GetSavePath(key);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        private static string GetGlobalString()
        {
            var path = GetGlobalSavePath();
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
        
        private static void SetString(string key, string value)
        {
            File.WriteAllText(GetSavePath(key), value);
        }

        private static void SetGlobalString(string value)
        {
            File.WriteAllText(GetGlobalSavePath(), value);
        }

        private static string GetSavePath(string fileName)
        {
            return string.Format("{0}/{1}{2}{3}",
                Application.persistentDataPath,
                FilePrefix,
                fileName,
                FileSuffix);
        }

        private static string GetGlobalSavePath()
        {
            return string.Format("{0}/{1}", Application.persistentDataPath, GlobalFileName);
        }
    }
}
