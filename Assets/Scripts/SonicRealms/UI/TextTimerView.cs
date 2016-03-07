using System;
using UnityEngine;
using UnityEngine.UI;

namespace SonicRealms.UI
{
    /// <summary>
    /// Formatted timer display.
    /// </summary>
    public class TextTimerView : TimerView
    {
        /// <summary>
        /// The text on which to show the timer.
        /// </summary>
        [Tooltip("The text on which to show the timer.")]
        public Text Text;

        private float _lastValue;

        /// <summary>
        /// How to format the current time. Formatting guide is here:
        /// https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
        /// 
        /// Common specifiers are:
        /// m: minutes
        /// ss: seconds
        /// ff: milliseconds
        /// </summary>
        [Tooltip("How to format the current time. Common specifiers are m for minutes, ss for seconds, and " +
                 "ff for milliseconds.")]
        public string Format;

        /// <summary>
        /// The timer will be updated only once a second if the format doesn't contain milliseconds.
        /// </summary>
        private bool _alwaysUpdate;

        /// <summary>
        /// The target animator.
        /// </summary>
        [Header("Animation")]
        [Tooltip("The target animator.")]
        public Animator Animator;

        /// <summary>
        /// Name of an Animator float set to the seconds on the timer.
        /// </summary>
        [Tooltip("Name of an Animator float set to the seconds on the timer.")]
        public string SecondsFloat;
        protected int SecondsFloatHash;

        public void Reset()
        {
            Text = GetComponentInChildren<Text>();
            Format = "mm:ss:ff";

            Animator = GetComponent<Animator>();
            SecondsFloat = "";
        }

        public void Awake()
        {
            _alwaysUpdate = Format.Contains("f");
            _lastValue = 0f;
        }

        public void Start()
        {
            Animator = Animator ? Animator : GetComponent<Animator>();
            SecondsFloatHash = Animator == null ? 0 : Animator.StringToHash(SecondsFloat);
        }

        public override void Show(TimeSpan time)
        {
            Display(time, Format);
        }

        public void Display(float seconds)
        {
            Show(TimeSpan.FromSeconds(seconds));
        }

        public void Display(float seconds, string format)
        {
            Display(TimeSpan.FromSeconds(seconds), format);
        }

        public void Display(TimeSpan time, string format)
        {
            if (Animator != null && SecondsFloatHash != 0)
                Animator.SetFloat(SecondsFloatHash, (float)time.TotalSeconds);

            if (!_alwaysUpdate && (int)time.TotalSeconds == (int)_lastValue) return;

            _lastValue = (int)time.TotalSeconds;
            Text.text = new DateTime().Add(time).ToString(format);
        }
    }
}
