using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.Core.Utils
{
    /// <summary>
    /// Sound helpers that can be called through unity events, animation events, and messages.
    /// </summary>
    public class SoundMessages : MonoBehaviour
    {
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

        public void PlayBGM(BGMLoopData data)
        {
            SoundManager.Instance.PlayBGM(data);
        }

        /// <summary>
        /// Stops the currently playing BGM only if the specified clip is playing.
        /// </summary>
        /// <param name="clip"></param>
        public void StopBGM(AudioClip clip)
        {
            SoundManager.Instance.StopBGM(clip);
        }

        /// <summary>
        /// Stops the currently playing BGM.
        /// </summary>
        public void StopBGM()
        {
            SoundManager.Instance.StopBGM();
        }

        /// <summary>
        /// Plays the specified audio clip as secondary background music invincibility music, for example) through the Sound Manager.
        /// </summary>
        /// <param name="clip">The specified audio clip.</param>
        public void PlaySecondaryBGM(AudioClip clip)
        {
            SoundManager.Instance.PlaySecondaryBGM(clip);
        }

        public void PlaySecondaryBGM(BGMLoopData data)
        {
            SoundManager.Instance.PlaySecondaryBGM(data);
        }

        /// <summary>
        /// Stops the secondary background music if the specified clip is playing.
        /// </summary>
        /// <param name="clip"></param>
        public void StopSecondaryBGM(AudioClip clip)
        {
            SoundManager.Instance.StopSecondaryBGM(clip);
        }

        /// <summary>
        /// Stops the secondary background music.
        /// </summary>
        public void StopSecondaryBGM()
        {
            SoundManager.Instance.StopSecondaryBGM();
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
