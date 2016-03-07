using UnityEngine;

namespace SonicRealms.Level
{
    [CreateAssetMenu(fileName = "New Character", menuName = "Sonic Realms/Character Data", order = 1)]
    public class CharacterData : ScriptableObject
    {
        [Header("Game Objects")]
        [Tooltip("The object used for the player when playing a level.")]
        public GameObject PlayerObject;

        [Tooltip("The object used for the signpost once the player passes it.")]
        public GameObject SignpostVictoryObject;

        [Header("UI")]
        [Tooltip("The name of the character as shown in character select.")]
        public string CharacterSelectName;

        [Tooltip("The name of the character as shown on the life counter.")]
        public string LifeCounterName;

        [Space]

        [Tooltip("The icon used for the character's life counter.")]
        public GameObject LifeCounterIcon;

        [Tooltip("The icon used for the character's saves in the save select screen.")]
        public GameObject SaveSelectIcon;
    }
}
