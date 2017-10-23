using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SonicRealms.Core.Internal
{
    public class SrUiNavigationCallback : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent _onSubmit;

        [SerializeField]
        private UnityEvent _onCancel;

        private float _previousHorizontal;
        private float _previousVertical;

        private StandaloneInputModule _inputModule;

        protected void Awake()
        {
            _onSubmit = _onSubmit ?? new UnityEvent();
            _onCancel = _onCancel ?? new UnityEvent();

            var inputModule = FindObjectOfType<StandaloneInputModule>();
            if (!inputModule)
            {
                Debug.LogError("There is no Standalone Input Module in the scene.");
                enabled = false;
                return;
            }

            _inputModule = inputModule;
        }

        protected void Update()
        {
            if (Input.GetButtonDown(_inputModule.submitButton))
                _onSubmit.Invoke();

            if(Input.GetButtonDown(_inputModule.cancelButton))
                _onCancel.Invoke();
        }
    }
}
