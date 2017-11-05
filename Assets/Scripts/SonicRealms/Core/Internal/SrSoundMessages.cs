using UnityEngine;

namespace SonicRealms.Core.Internal
{
    /// <summary>
    /// Sound helpers that can be called through unity events, animation events, and messages.
    /// </summary>
    public class SrSoundMessages : MonoBehaviour
    {
        /// <summary>
        /// Plays the specified audio clip at the object's position through the Sound Manager.
        /// </summary>
        /// <param name="clip">The specified audio clip.</param>
        public void PlayAudioClip(AudioClip clip)
        {
            SrSoundManager.PlaySoundEffect(clip);
        }

        /// <summary>
        /// Plays the specified audio clip as background music through the Sound Manager.
        /// </summary>
        /// <param name="clip">The specified audio clip.</param>
        public void PlayBGM(AudioClip clip)
        {
            SrSoundManager.PlayMainMusic(clip);
        }

        /// <summary>
        /// Stops the currently playing BGM only if the specified clip is playing.
        /// </summary>
        /// <param name="clip"></param>
        public void StopBGM(AudioClip clip)
        {
            if (SrSoundManager.MainMusicIs(clip))
                SrSoundManager.StopMainMusic();
        }

        /// <summary>
        /// Stops the currently playing BGM.
        /// </summary>
        public void StopBGM()
        {
            SrSoundManager.StopMainMusic();
        }

        /// <summary>
        /// Plays the specified audio clip as secondary background music invincibility music, for example) through the Sound Manager.
        /// </summary>
        /// <param name="clip">The specified audio clip.</param>
        public void PlaySecondaryBGM(AudioClip clip)
        {
            SrSoundManager.PlayPowerupMusic(clip);
        }

        /// <summary>
        /// Stops the secondary background music if the specified clip is playing.
        /// </summary>
        /// <param name="clip"></param>
        public void StopSecondaryBGM(AudioClip clip)
        {
            if (SrSoundManager.PowerupMusicIs(clip))
                SrSoundManager.StopPowerupMusic();
        }

        /// <summary>
        /// Stops the secondary background music.
        /// </summary>
        public void StopSecondaryBGM()
        {
            SrSoundManager.StopPowerupMusic();
        }

        /// <summary>
        /// Plays the specified audio clip as a jingle (extra life music, for example
        /// </summary>
        /// <param name="clip"></param>
        public void PlayJingle(AudioClip clip)
        {
            SrSoundManager.PlayJingle(clip);
        }
    }
}
