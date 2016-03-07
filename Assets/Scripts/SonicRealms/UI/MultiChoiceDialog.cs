using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SonicRealms.UI
{
    public class MultiChoiceDialog : BaseDialog
    {
        /// <summary>
        /// Invoked when the selected choice changes.
        /// </summary>
        public DialogChoiceEvent OnChangeSelection;

        /// <summary>
        /// The currently selected choice object.
        /// </summary>
        [Space]
        [Tooltip("The currently selected choice object.")]
        public DialogChoiceObject SelectedChoiceObject;

        protected List<DialogChoiceObject> DialogChoiceObjects;

        /// <summary>
        /// The input axis used to switch between the two choices.
        /// </summary>
        [Tooltip("The input axis used to switch between the two choices.")]
        public string SelectionInputAxis;
        protected bool SelectionInputAxisUsed;

        /// <summary>
        /// The input button used to accept the currently selected choice.
        /// </summary>
        [Tooltip("The input button used to accept the currently selected choice.")]
        public string AcceptChoiceButton;

        /// <summary>
        /// If true, the dialog will not respond to input.
        /// </summary>
        [Tooltip("If checked, the dialog will not respond to input.")]
        public bool DisableInput;

        /// <summary>
        /// Animator that will receive info from this component.
        /// </summary>
        [Space]
        [Tooltip("Animator that will receive info from this component.")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator trigger set when the selection is changed.
        /// </summary>
        [Tooltip("Name of an Animator trigger set when the selection is changed.")]
        public string ChangeSelectionTrigger;
        protected int ChangeSelectionTriggerHash;

        /// <summary>
        /// Name of an Animator int set to the index of the current selection.
        /// </summary>
        [Tooltip("Name of an Animator int set to the index of the current selection.")]
        public string SelectionIndexInt;
        protected int SelectionIndexIntHash;

        public override void Reset()
        {
            base.Reset();

            SelectionInputAxis = "Horizontal";
            AcceptChoiceButton = "Submit";

            Animator = GetComponent<Animator>();
            ChangeSelectionTrigger = "";
            SelectionIndexInt = "";

            OnChangeSelection = new DialogChoiceEvent();
        }

        public override void Awake()
        {
            base.Awake();

            DialogChoiceObjects = new List<DialogChoiceObject>();

            ChangeSelectionTriggerHash = Animator.StringToHash(ChangeSelectionTrigger);
            SelectionIndexIntHash = Animator.StringToHash(SelectionIndexInt);

            OnChangeSelection = OnChangeSelection ?? new DialogChoiceEvent();
        }

        public void OnTransformChildrenChanged()
        {
            GetDialogChoiceObjects();
        }

        public override void Open()
        {
            base.Open();
            if(SelectedChoiceObject != null) Select(SelectedChoiceObject);
        }

        public void GetDialogChoiceObjects()
        {
            DialogChoiceObjects.Clear();

            foreach (var t in transform)
            {
                var dialogChoiceObject = (t as Transform).GetComponent<DialogChoiceObject>();
                if (dialogChoiceObject != null)
                {
                    DialogChoiceObjects.Add(dialogChoiceObject);
                }
            }
        }

        public void Select(DialogChoiceObject dialogChoiceObject)
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(SelectedChoiceObject.gameObject);

            if (dialogChoiceObject == SelectedChoiceObject) return;

            SelectedChoiceObject = dialogChoiceObject;
            OnChangeSelection.Invoke(SelectedChoiceObject.Choice);

            if (Animator != null)
            {
                if(SelectionIndexIntHash != 0)
                    Animator.SetInteger(SelectionIndexIntHash, DialogChoiceObjects.IndexOf(dialogChoiceObject));

                if (ChangeSelectionTriggerHash != 0)
                    Animator.SetTrigger(ChangeSelectionTriggerHash);
            }
        }
    }
}
