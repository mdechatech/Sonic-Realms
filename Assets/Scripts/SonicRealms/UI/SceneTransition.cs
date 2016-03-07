using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SonicRealms.UI
{
    /// <summary>
    /// For animations that play before changing scenes.
    /// </summary>
    public class SceneTransition : Transition
    {
        /// <summary>
        /// If transitioning, the index of the target scene.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("If transitioning, the index of the target scene.")]
        public int TargetIndex;

        /// <summary>
        /// If transitioning, the name of the target scene.
        /// </summary>
        [Foldout("Debug")]
        [Tooltip("If transitioning, the name of the target scene.")]
        public string TargetName;

        /// <summary>
        /// Invoked when the transition is halfway complete. At this point, the new scene is loaded.
        /// </summary>
        [Foldout("Events")]
        [Tooltip("Invoked when the transition is halfway complete. At this point, the new scene is loaded.")]
        public UnityEvent OnNextScene;

        public override void Awake()
        {
            base.Awake();
            OnNextScene = OnNextScene ?? new UnityEvent();
            DontDestroyOnLoad(gameObject);
        }

        public void Go(Scene scene)
        {
            TargetName = scene.name;
            TargetIndex = scene.buildIndex;
            Time.timeScale = 0f;
            Enter();
            OnGo();
        }

        public void Go(string sceneName)
        {
            TargetName = sceneName;
            TargetIndex = -1;
            Time.timeScale = 0f;
            Enter();
            OnGo();
        }

        public void Go(int sceneIndex)
        {
            TargetName = null;
            TargetIndex = sceneIndex;
            Time.timeScale = 0f;
            Enter();
            OnGo();
        }

        protected virtual void OnGo()
        {

        }

        public override void EnterComplete()
        {
            base.EnterComplete();
            NextScene();
        }

        protected virtual void NextScene()
        {
            Exit();
            if (TargetIndex >= 0)
            {
                SceneManager.LoadScene(TargetIndex);
            }
            else if (!string.IsNullOrEmpty(TargetName))
            {
                SceneManager.LoadScene(TargetName);
            }
            else
            {
                Debug.LogError("There was no target scene to transition to.");
            }
        }

        public virtual void OnLevelWasLoaded(int level)
        {
            OnNextScene.Invoke();
            TargetIndex = -1;
            TargetName = null;
        }
    }
}
