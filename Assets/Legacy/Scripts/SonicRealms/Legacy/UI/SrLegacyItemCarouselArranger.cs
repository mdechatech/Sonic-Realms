using System.Collections;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    public abstract class SrLegacyItemCarouselArranger : MonoBehaviour
    {
        public SrLegacyItemCarousel Carousel { get; set; }

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
