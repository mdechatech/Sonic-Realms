using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SonicRealms.Core.Utils
{
    public class PreventUiMouseInput : MonoBehaviour
    {
        private GameObject _previousSelected;

        protected void Start()
        {
            var raycasters = FindObjectsOfType<GraphicRaycaster>();
            for (var i = 0; i < raycasters.Length; ++i)
            {
                raycasters[i].enabled = false;
            }
        }

        protected void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(_previousSelected);
            }
            else
            {
                _previousSelected = EventSystem.current.currentSelectedGameObject;
            }
        }
    }
}
