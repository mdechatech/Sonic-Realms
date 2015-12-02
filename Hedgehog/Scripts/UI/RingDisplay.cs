using Hedgehog.Core.Actors;
using UnityEngine;
using UnityEngine.UI;

namespace Hedgehog.UI
{
    [RequireComponent(typeof(Text))]
    public class RingDisplay : MonoBehaviour
    {
        public Text Text;
        public RingCollector Target;

        public void Reset()
        {
            Text = GetComponent<Text>();
            Target = FindObjectOfType<RingCollector>();
        }

        public void Display(int value)
        {
            Text.text = value.ToString();
        }

        public void Update()
        {
            if (Target == null) return;
            Text.text = Target.Amount.ToString();
        }
    }
}
