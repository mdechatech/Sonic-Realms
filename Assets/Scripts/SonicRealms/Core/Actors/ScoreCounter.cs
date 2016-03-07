using SonicRealms.UI;
using UnityEngine;
using UnityEngine.Events;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Keeps track of score and score combos.
    /// </summary>
    public class ScoreCounter : MonoBehaviour
    {
        /// <summary>
        /// Current score.
        /// </summary>
        [SerializeField]
        [Tooltip("Current score.")]
        private int _score;

        /// <summary>
        /// Current score.
        /// </summary>
        public int Score
        {
            get { return _score; }
            set
            {
                _score = value; 
                OnValueChange.Invoke();
            }
        }

        /// <summary>
        /// The combo number the score is currently on.
        /// </summary>
        [Tooltip("The combo number the score is currently on.")]
        public int CurrentCombo;

        /// <summary>
        /// Score values for each combo number. The 0th item maps to the 1st combo.
        /// </summary>
        [Tooltip("Score values for each combo number. The 0th item maps to the 1st combo.")]
        public int[] ComboValues;

        /// <summary>
        /// Game object to make copies of when showing floating scores.
        /// </summary>
        [Tooltip("Game object to make copies of when showing floating scores.")]
        public ScoreView FloatingScoreView;

        /// <summary>
        /// Invoked when the score changes.
        /// </summary>
        public UnityEvent OnValueChange;

        public void Reset()
        {
            _score = 0;

            CurrentCombo = 0;
            ComboValues = new []
            {
                100,
                200,
                500,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                10000,
            };

            OnValueChange = new UnityEvent();
        }

        public void Awake()
        {
            OnValueChange = OnValueChange ?? new UnityEvent();
        }

        /// <summary>
        /// Adds to the current score combo using the given source.
        /// </summary>
        /// <param name="source">The source of the combo increase. This is usually a destroyed enemy.</param>
        public void AddCombo(Transform source)
        {
            var value = ComboValues[Mathf.Min(CurrentCombo++, ComboValues.Length - 1)];
            Score += value;

            if (source == null || FloatingScoreView == null) return;

            var score = Instantiate(FloatingScoreView);
            score.transform.position = source.position;
            score.Show(value);
        }

        /// <summary>
        /// Ends the current score combo.
        /// </summary>
        public void EndCombo()
        {
            CurrentCombo = 0;
        }
    }
}
