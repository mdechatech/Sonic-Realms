using System.Collections;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public class SrLegacyBlinkItemCarouselArranger : SrLegacyItemCarouselArranger
    {
        [SerializeField]
        private Transform _itemContainer;

        [SerializeField]
        private Transform _selectionCenter;

        [SerializeField]
        private bool _worldPositionStays;

        public override void SnapSelection(int oldSelectedIndex, int newSelectedIndex)
        {
            var selectedItem = Carousel.SelectedItem;
            selectedItem.SetActive(true);

            for (var i = 0; i < Carousel.ItemCount; ++i)
            {
                if (i != newSelectedIndex)
                    Carousel[i].gameObject.SetActive(false);
            }
        }

        public override IEnumerator ChangeSelection(int oldSelectedIndex, int newSelectedIndex)
        {
            SnapSelection(oldSelectedIndex, newSelectedIndex);
            yield break;
        }

        public override void PlaceItem(GameObject item, int index)
        {
            item.transform.SetParent(_itemContainer, _worldPositionStays);

            if (index != Carousel.SelectedIndex)
                item.SetActive(false);
        }

        protected virtual void Reset()
        {
            _itemContainer = transform;
            _selectionCenter = transform;
        }

        protected virtual void Awake()
        {
            _itemContainer = _itemContainer ?? transform;
            _selectionCenter = _selectionCenter ?? transform;
        }
    }
}
