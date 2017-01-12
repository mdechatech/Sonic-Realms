using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    public class VolumePicker : MonoBehaviour
    {
        [SerializeField]
        private Slider _masterSlider;

        [SerializeField]
        private Slider _musicSlider;

        [SerializeField]
        private Slider _fxSlider;

        protected void Start()
        {
            _masterSlider.value = SoundManager.MasterVolume;
            _masterSlider.onValueChanged.AddListener(value =>
            {
                SoundManager.MasterVolume = value;
                GameManager.Instance.FlushSoundSettings();
            });

            _musicSlider.value = SoundManager.MusicVolume;
            _musicSlider.onValueChanged.AddListener(value =>
            {
                SoundManager.MusicVolume = value;
                GameManager.Instance.FlushSoundSettings();
            });

            _fxSlider.value = SoundManager.FxVolume;
            _fxSlider.onValueChanged.AddListener(value =>
            {
                SoundManager.FxVolume = value;
                GameManager.Instance.FlushSoundSettings();
            });
        }
    }
}
