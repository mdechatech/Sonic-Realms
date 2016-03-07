using System.Linq;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Helper component with methods that can be called through SendMessage, UnityEvent, and AnimationEvent.
    /// </summary>
    public class AnimationEvents : MonoBehaviour
    {
        /// <summary>
        /// Destroys the specified game object.
        /// </summary>
        /// <param name="gameObject">The specified game object.</param>
        public void DestroyGameObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Destroys the specified component.
        /// </summary>
        /// <param name="component">The specified component.</param>
        public void DestroyComponent(Component component)
        {
            Destroy(component);
        }

        /// <summary>
        /// Destroys this component's game object.
        /// </summary>
        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Copies the specified object at the transform's position, at the top level of the hierarchy
        /// </summary>
        /// <param name="original">The specified object.</param>
        public void AddObject(GameObject original)
        {
            Instantiate(original, transform.position, transform.rotation);
        }

        /// <summary>
        /// Copies the specified object at the transform's position, at a child of the transform.
        /// </summary>
        /// <param name="original">The specified object.</param>
        public void AddObjectAsChild(GameObject original)
        {
            var copy = (GameObject)Instantiate(original, transform.position, transform.rotation);
            copy.transform.position = transform.position;
        }

        /// <summary>
        /// Disables the component with the specified type name.
        /// </summary>
        /// <param name="type">The specified type name.</param>
        public void DisableComponent(string type)
        {
            var component = GetComponents<Behaviour>().
                FirstOrDefault(component1 => component1.GetType().Name == type);
            if (component != null) component.enabled = false;
        }

        /// <summary>
        /// Enables the component with the specified type name.
        /// </summary>
        /// <param name="type">The specified type name.</param>
        public void EnableComponent(string type)
        {
            var component = GetComponents<Behaviour>().
                FirstOrDefault(component1 => component1.GetType().Name == type);
            if (component != null)
                component.enabled = true;
        }

        /// <summary>
        /// Sets the sorting order of the sprite.
        /// </summary>
        public void SetSortingOrder(int sortingOrder)
        {
            var sprite = GetComponentInChildren<SpriteRenderer>();
            if (sprite == null) return;
            sprite.sortingOrder = sortingOrder;
        }

        /// <summary>
        /// Plays the specified audio clip at the object's position through the Sound Manager.
        /// </summary>
        /// <param name="clip">The specified audio clip.</param>
        public void PlayAudioClip(AudioClip clip)
        {
            SoundManager.Instance.PlayClipAtPoint(clip, transform.position);
        }

        /// <summary>
        /// Plays the specified audio clip as background music through the Sound Manager.
        /// </summary>
        /// <param name="clip">The specified audio clip.</param>
        public void PlayBGM(AudioClip clip)
        {
            SoundManager.Instance.PlayBGM(clip);
        }

        /// <summary>
        /// Plays the specified audio clip as secondary background music invincibility music, for example) through the Sound Manager.
        /// </summary>
        /// <param name="clip">The specified audio clip.</param>
        public void PlaySecondaryBGM(AudioClip clip)
        {
            SoundManager.Instance.PlaySecondaryBGM(clip);
        }

        /// <summary>
        /// Plays the specified audio clip as a jingle (extra life music, for example
        /// </summary>
        /// <param name="clip"></param>
        public void PlayJingle(AudioClip clip)
        {
            SoundManager.Instance.PlayJingle(clip);
        }
    }
}
