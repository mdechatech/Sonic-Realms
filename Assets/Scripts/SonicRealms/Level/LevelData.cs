using UnityEngine;

namespace SonicRealms.Level
{
    [CreateAssetMenu(fileName = "New Level", menuName = "Sonic Realms/Level Data", order = 0)]
    public class LevelData : ScriptableObject
    {
        /// <summary>
        /// The icon used for this level in the level select screen.
        /// </summary>
        [Tooltip("The icon used for this level in the level select screen.")]
        public GameObject LevelSelectIcon;

        /// <summary>
        /// The name of the scene which holds the level. This is the name as it shows up in Build Settings.
        /// </summary>
        [Tooltip("The name of the scene which holds the level. This is the name as it shows up in Build Settings.")]
        public string Scene;

        /// <summary>
        /// The name of the level manager object in the scene. It must have a Level Manager component attached.
        /// </summary>
        [Tooltip("The name of the level manager object in the scene. It must have a Level Manager component attached.")]
        public string LevelManager;
    }
}
