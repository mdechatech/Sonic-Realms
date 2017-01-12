using System.Collections;
using UnityEngine;

namespace SonicRealms.UI
{
    public abstract class ItemCarouselArranger : MonoBehaviour
    {
        public ItemCarousel Carousel { get; set; }

        public abstract void PlaceItem(GameObject item, int index);

        public abstract void SnapSelection(int oldSelectedIndex, int newSelectedIndex);

        public abstract IEnumerator ChangeSelection(int oldSelectedIndex, int newSelectedIndex);

        protected virtual void Start()
        {
            if (!Carousel)
                Debug.LogError(string.Format("Arranger '{0}' hasn't been assigned to an Item Carousel.", name));
        }
    }
}
