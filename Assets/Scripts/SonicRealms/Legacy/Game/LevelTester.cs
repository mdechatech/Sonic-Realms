using SonicRealms.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SonicRealms.Legacy.Game
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
        public LevelManager LevelManager;
        public GameManager BaseGameManager;

        protected GameManager GameManager;

        protected void Start()
        {
            if (GameManager.Instance != null)
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
            var tester = FindObjectOfType<LevelTester>();
            if (!tester)
                return;

            if (tester.LevelManager is GoalLevelManager)
            {
                var goal = (GoalLevelManager) tester.LevelManager;
                goal.Lives = 99;
                goal.StartLevelCalled.AddListener(() => goal.Lives = 99);
            }
        }
    }
}
