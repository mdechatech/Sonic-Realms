using System.Collections.Generic;
using System.Linq;
using SonicRealms.Core.Utils;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.UI
{
    public class SaveDataPicker : MonoBehaviour
    {
        /// <summary>
        /// Name of the input button used to accept the selected save item.
        /// </summary>
        [Tooltip("Name of the input button used to accept the selected save item.")]
        public string OKButton;

        /// <summary>
        /// The item picker from which to get the selected save item.
        /// </summary>
        [Tooltip("The item picker from which to get the selected save item.")]
        public LinearItemPicker Picker;

        /// <summary>
        /// This object will display save information whenever a new item is selected.
        /// </summary>
        [Tooltip("This object will display save information whenever a new item is selected.")]
        public BaseSaveDataPreview InfoPreview;
        
        /// <summary>
        /// The object that will be used for the save items.
        /// </summary>
        [Tooltip("The object that will be used for the save items.")]
        public BaseSaveDataPreview BaseSaveItem;

        /// <summary>
        /// This cursor goes over the currently selected object.
        /// </summary>
        [Tooltip("This cursor goes over the currently selected object.")]
        public GameObject SelectionCursor;

        /// <summary>
        /// The list of file names, each of which will be shown as an item in the save picker.
        /// </summary>
        [Tooltip("The list of file names, each of which will be shown as an item in the save picker.")]
        public List<string> SaveNames;

        protected Dictionary<GameObject, SaveData> Saves;
        protected Dictionary<GameObject, string> Names;

            /// <summary>
        /// This dialog is displayed before creating a new game. Nothing happens unless the dialog's choice
        /// is DialogChoice.Yes.
        /// </summary>
        [Space]
        [Tooltip("This dialog is displayed before creating a new game. Nothing happens unless the dialog's " +
                 "choice is DialogChoice.Yes.")]
        public MultiChoiceDialog NewGameDialog;

        /// <summary>
        /// This dialog is displayed before continuing a saved game. Nothing happens unless the dialog's choice
        /// is DialogChoice.No.
        /// </summary>
        [Tooltip("This dialog is displayed before continuing a saved game. Nothing happens unless the dialog's " +
                 "choice is DialogChoice.No.")]
        public MultiChoiceDialog LoadGameDialog;

        [HideInInspector] public bool DisableInput;

        public void Reset()
        {
            OKButton = "Submit";
            Picker = GetComponentInChildren<LinearItemPicker>();
            
            InfoPreview = null;
            BaseSaveItem = null;
            SelectionCursor = null;

            SaveNames = new List<string>
            {
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9"
            };
        }

        public void Awake()
        {
            Saves = new Dictionary<GameObject, SaveData>();
            Names = new Dictionary<GameObject, string>();
        }

        public void Start()
        {
            Picker = Picker ?? GetComponentInChildren<LinearItemPicker>();
            Picker.OnSelect.AddListener(OnSelect);

            CreateItems();
        }

        /// <summary>
        /// Creates the save items to show in the picker.
        /// </summary>
        public void CreateItems()
        {
            foreach (var fileName in SaveNames)
            {
                var item = Instantiate(BaseSaveItem);
                item.transform.SetParent(Picker.transform, false);

                Saves.Add(item.gameObject, SaveManager.Load(fileName));
                Names.Add(item.gameObject, fileName);
            }
        }

        public void Update()
        {
            if (SelectionCursor)
                UpdateSelectionCursor();

            if(!DisableInput && !NewGameDialog.IsOpen)
                CheckOKInput();
        }

        /// <summary>
        /// Checks for the press of the "OK" button.
        /// </summary>
        public void CheckOKInput()
        {
            if (Input.GetButtonDown(OKButton))
            {
                var saveData = Saves[Picker.SelectedObject];
                if (saveData == null)
                {
                    OpenNewGameDialog();
                }
                else
                {
                    OpenLoadGameDialog();
                }
            }
        }

        /// <summary>
        /// Opens the confirmation dialog for making a new game.
        /// </summary>
        public void OpenNewGameDialog()
        {
            Picker.DisableInput = true;
            NewGameDialog.OnClose.AddListener(OnNewGameDialogClose);
            NewGameDialog.Open();
        }

        public void OnNewGameDialogClose(DialogChoice choice)
        {
            if (choice == DialogChoice.Yes)
            {
                DisableInput = true;
                GameManager.Instance.StartNewGame(Names[Picker.SelectedObject]);
            }

            Picker.DisableInput = false;
            NewGameDialog.OnClose.RemoveListener(OnNewGameDialogClose);

            Input.ResetInputAxes();
        }

        /// <summary>
        /// Closes the new game confirmation dialog.
        /// </summary>
        public void CancelNewGameDialog()
        {
            Picker.DisableInput = false;
            NewGameDialog.Close();
        }

        /// <summary>
        /// Opens the confirmation dialog for loading a game.
        /// </summary>
        public void OpenLoadGameDialog()
        {
            Picker.DisableInput = true;
            LoadGameDialog.OnClose.AddListener(OnLoadGameDialogClose);
            LoadGameDialog.Open();
        }

        public void OnLoadGameDialogClose(DialogChoice choice)
        {
            if (choice == DialogChoice.Yes)
            {
                DisableInput = true;
                GameManager.Instance.LoadGame(Saves[Picker.SelectedObject]);
                if (GameManager.Instance.LevelData != null)
                    GameManager.Instance.LoadLevel(GameManager.Instance.LevelData);
                else 
                    GameManager.Instance.LoadLevel(GameManager.Instance.Levels.FirstOrDefault());
            }

            Picker.DisableInput = false;
            LoadGameDialog.OnClose.RemoveListener(OnLoadGameDialogClose);

            Input.ResetInputAxes();
        }

        /// <summary>
        /// Closes the load game confirmation dialog.
        /// </summary>
        public void CancelLoadGameDialog()
        {
            Picker.DisableInput = false;
            LoadGameDialog.Close();
        }

        /// <summary>
        /// Updates the position of the selection cursor to highlight the currently selected save item.
        /// </summary>
        public void UpdateSelectionCursor()
        {
            if (Picker.SelectedObject == null)
            {
                SelectionCursor.SetActive(false);
            }
            else
            {
                SelectionCursor.SetActive(true);
                SelectionCursor.transform.position = Picker.SelectedObject.transform.position;
            }
        }
            
        public void OnSelect()
        {
            if(InfoPreview)
                InfoPreview.Display(Saves[Picker.SelectedObject]);
        }
    }
}
