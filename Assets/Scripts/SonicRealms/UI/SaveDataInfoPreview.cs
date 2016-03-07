using System;
using SonicRealms.Level;
using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    public class SaveDataInfoPreview : BaseSaveDataPreview
    {
        /// <summary>
        /// Screen to display when showing a save that doesn't exist (new game).
        /// </summary>
        [Tooltip("Screen to display when showing a save that doesn't exist (new game).")]
        public GameObject NewSaveScreen;

        /// <summary>
        /// Screen to display when showing an existing save (load game).
        /// </summary>
        [Tooltip("Screen to display when showing an existing save (load game).")]
        public GameObject FoundSaveScreen;

        [Space]

        public Transform LevelIconContainer;
        protected GameObject LevelIconObject;

        [Space]

        public Text CharacterText;
        public Text LivesText;
        public Text ScoreText;
        public Text RingsText;

        [Space]

        public Text TimeText;
        public string TimeFormat;

        public void Awake()
        {
            
        }

        public override void Display(SaveData saveData)
        {
            if (saveData == null)
            {
                NewSaveScreen.SetActive(true);
                FoundSaveScreen.SetActive(false);
            }
            else
            {
                NewSaveScreen.SetActive(false);
                FoundSaveScreen.SetActive(true);
                
                if(LevelIconObject) Destroy(LevelIconObject);
                LivesText.text = saveData.Lives.ToString();
                ScoreText.text = saveData.Score.ToString();
                RingsText.text = saveData.Rings.ToString();
                TimeText.text = new DateTime().Add(TimeSpan.FromSeconds(saveData.Time)).ToString(TimeFormat);

                var characterData = GameManager.Instance.GetCharacterData(saveData.Character);
                if (characterData != null)
                {
                    CharacterText.text = characterData.CharacterSelectName;
                }
                else
                {
                    CharacterText.text = "";
                }
                
            }
        }
    }
}
