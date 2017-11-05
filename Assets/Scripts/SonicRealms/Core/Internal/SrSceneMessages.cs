using UnityEngine;
using UnityEngine.SceneManagement;

namespace SonicRealms.Core.Internal
{
    /// <summary>
    /// Scene helpers that can be called through unity events, animation events, and messages.
    /// </summary>
    public class SrSceneMessages : MonoBehaviour
    {
        public void CreateScene(string sceneName)
        {
            SceneManager.CreateScene(sceneName);
        }

        public void LoadScene(int sceneBuildIndex)
        {
            SceneManager.LoadScene(sceneBuildIndex);
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void UnloadScene(int sceneBuildIndex)
        {
            SceneManager.UnloadScene(sceneBuildIndex);
        }

        public void UnloadScene(string sceneName)
        {
            SceneManager.UnloadScene(sceneName);
        }
    }
}
