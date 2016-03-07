using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SonicRealms.Core.Utils
{
    public static class GameObjectUtility
    {
        /// <summary>
        /// Returns the path of the specified game object. This path can be used in GameObject.Find.
        /// </summary>
        /// <param name="gameObject">The specified game object.</param>
        /// <returns></returns>
        public static string GetPath(GameObject gameObject)
        {
            if (gameObject == null) return null;
            if (gameObject.transform == gameObject.transform.root) return gameObject.name;

            var chain = new List<Transform>();
            var check = gameObject.transform;

            do
            {
                chain.Add(check);
                check = check.parent;
            } while (check != null);

            var result = new StringBuilder();
            for (var i = chain.Count - 1; i >= 0; --i)
            {
                result.Append(chain[i].name);
                result.Append('/');
            }

            return result.ToString();
        }

        // A GameObject.Find that has the option to include inactive objects.
        public static GameObject Find(string name, bool includeInactive = false)
        {
            // Just use the normal GameObject.Find if we're only looking for active objects
            if (!includeInactive) return GameObject.Find(name);

            // Split up the path
            string parent, children;
            int index;
            if ((index = name.IndexOf('/')) >= 0)
            {
                parent = name.Substring(0, index);
                children = name.Substring(index + 1);
            }
            else
            {
                parent = name;
                children = null;
            }

            // Scene is loaded - loop through the root game objects from the active scene and call transform.Find
            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name != parent) continue;
                    if (children == null) return root;

                    var child = root.transform.Find(children);
                    if (child == null) return null;
                    return child.gameObject;
                }

                return null;
            }
            // Scene isn't loaded - loop through all root game objects using FindObjectsOfType and call transform.Find
            // VERY SLOW, but this code will be executed only at the beginning of a scene
            else
            {
                foreach (var transform in GameObject.FindObjectsOfType<Transform>())
                {
                    var root = transform.root;
                    if (root != transform) continue;
                    if (root.name != parent) continue;

                    if (children == null) return root.gameObject;
                    var child = root.Find(children);

                    if (child == null) return null;
                    return child.gameObject;
                }

                return null;
            }
        }
    }
}
