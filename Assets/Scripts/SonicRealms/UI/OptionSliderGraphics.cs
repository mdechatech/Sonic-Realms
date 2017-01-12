using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    [RequireComponent(typeof(Slider))]
    public class OptionSliderGraphics : MonoBehaviour
    {
        [SerializeField]
        private OptionSliderLabelBase _label;

        private Slider _slider;

        private EventTrigger.TriggerEvent _onSelect;
        private EventTrigger.TriggerEvent _onDeselect;

        protected void Awake()
        {
            _slider = GetComponent<Slider>();

            _onSelect = new EventTrigger.TriggerEvent();
            _onSelect.AddListener(Slider_OnSelect);

            _onDeselect = new EventTrigger.TriggerEvent();
            _onDeselect.AddListener(Slider_OnDeselect);
        }

        protected void Start()
        {
            var eventTrigger = _slider.GetComponent<EventTrigger>() ?? _slider.gameObject.AddComponent<EventTrigger>();

            eventTrigger.triggers.Add(new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select,
                callback = _onSelect
            });

            eventTrigger.triggers.Add(new EventTrigger.Entry
            {
                eventID = EventTriggerType.Deselect,
                callback = _onDeselect
            });
        }

        protected void OnDisable()
        {
            _label.Unfocus();
        }

        private void Slider_OnSelect(BaseEventData e)
        {
            _label.Focus();
        }

        private void Slider_OnDeselect(BaseEventData e)
        {
            _label.Unfocus();
        }
    }
}
