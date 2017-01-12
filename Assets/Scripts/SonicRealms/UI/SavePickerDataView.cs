using System;
using SonicRealms.Level;
using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    public class SavePickerDataView : SaveDataViewBase
    {
        [SerializeField]
        private Text _characterText;

        [SerializeField]
        private Text _livesText;

        [SerializeField]
        private Text _ringsText;

        [SerializeField]
        private Text _scoreText;

        [SerializeField]
        private Text _timeText;


        protected override void UpdateSaveData(SaveData oldValue, SaveData newValue)
        {
            if (newValue != null)
            {
                var characterData = GameManager.Instance.GetCharacterData(newValue.Character);
                if (characterData != null)
                {
                    _characterText.text = characterData.CharacterSelectName;
                }
                else
                {
                    _characterText.text = string.Empty;
                }

                _livesText.text = newValue.Lives.ToString();
                _ringsText.text = newValue.Rings.ToString();
                _scoreText.text = newValue.Score.ToString();
                _timeText.text = new DateTime().Add(TimeSpan.FromSeconds(newValue.Time))
                    .ToString("mm:ss");
            }
            else
            {
                _characterText.text = string.Empty;
                _livesText.text = string.Empty;
                _ringsText.text = string.Empty;
                _scoreText.text = string.Empty;
                _timeText.text = string.Empty;
            }
        }
    }
}
