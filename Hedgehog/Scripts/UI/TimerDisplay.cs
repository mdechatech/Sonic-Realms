using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hedgehog.UI
{
    [RequireComponent(typeof(Text))]
    public class TimerDisplay : MonoBehaviour
    {
        public Text Text;
        public string Format;

        public void Reset()
        {
            Text = GetComponent<Text>();
            Format = "mm:ss:ff";
        }

        public void Display(DateTime time)
        {
            Text.text = time.ToString(Format);
        }

        public void Display(DateTime time, string format)
        {
            Text.text = time.ToString(format);
        }

        public void Display(float seconds)
        {
            Text.text = new DateTime().Add(TimeSpan.FromSeconds(seconds)).ToString(Format);
        }

        public void Display(float seconds, string format)
        {
            Text.text = new DateTime().Add(TimeSpan.FromSeconds(seconds)).ToString(format);
        }
    }
}
