using SonicRealms.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SonicRealms.Legacy.Game
{
    /// <summary>
    /// Tests the given level without having to go through the game manager first.
    /// </summary>
    public class SrLegacyLevelTester : MonoBehaviour
    {
        [Header("Test Data")]
        public SrLegacyLevelData Level;
        public SrLegacyCharacterData Character;

        [Header("Infrastructure")]
        public SrLegacyLevelManager LevelManager;
        public SrLegacyGameManager BaseGameManager;

        protected SrLegacyGameManager GameManager;

        protected void Start()
        {
            if (SrLegacyGameManager.Instance != null)
            {
                return;
            }

            SceneManager.sceneLoaded += (a, b) => SetConditions();

            var gameManager = Instantiate(BaseGameManager);
            gameManager.name = BaseGameManager.name;
            gameManager.CharacterData = Character;
            gameManager.LoadLevel(Level);
        }

        private static void SetConditions()
        {
            var tester = FindObjectOfType<SrLegacyLevelTester>();
            if (!tester)
                return;

            if (tester.LevelManager is SrLegacyGoalLevelManager)
            {
                var goal = (SrLegacyGoalLevelManager) tester.LevelManager;
                goal.Lives = 99;
                goal.StartLevelCalled.AddListener(() => goal.Lives = 99);
            }
        }
    }
}
