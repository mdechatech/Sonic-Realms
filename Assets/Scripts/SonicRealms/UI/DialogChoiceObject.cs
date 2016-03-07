using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public class DialogChoiceObject : MonoBehaviour, ISubmitHandler, IPointerClickHandler
    {
        public DialogChoice Choice;
        [HideInInspector] public Selectable Selectable;

        protected BaseDialog Dialog;

        public void Reset()
        {
            Choice = DialogChoice.None;
        }

        public void Awake()
        {
            Selectable = GetComponent<Selectable>();
        }

        public void Start()
        {
            Dialog = transform.parent.GetComponent<BaseDialog>();
        }

        public void OnEnable()
        {
            if (Dialog == null) Dialog = transform.parent.GetComponent<BaseDialog>();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Dialog.Close(Choice);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Dialog.Close(Choice);
        }
    }
}
