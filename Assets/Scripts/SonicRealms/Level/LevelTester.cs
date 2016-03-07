using UnityEngine;

namespace SonicRealms.Level
{
    /// <summary>
    /// Tests the given level without having to go through the game manager first.
    /// </summary>
    public class LevelTester : MonoBehaviour
    {
        [Header("Test Data")]
        public LevelData Level;
        public CharacterData Character;

        [Header("Infrastructure")]
        public GameManager BaseGameManager;
        public SoundManager BaseSoundManager;

        public GameManager GameManager;

        public void Awake()
        {
            if (GameManager.Instance != null)
            {
                return;
            }

            Instantiate(BaseSoundManager).name = BaseSoundManager.name;
            
            var gameManager = Instantiate(BaseGameManager);
            gameManager.name = BaseGameManager.name;
            gameManager.CharacterData = Character;
            gameManager.LoadLevel(Level);
        }
    }
}
