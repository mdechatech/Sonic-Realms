using System.ComponentModel;
using SonicRealms.Core.Internal;
using SonicRealms.Core.Utils;
using SonicRealms.Legacy.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SonicRealms.Core.Actors
{
    /// <summary>
    /// Keeps track of score and score combos.
    /// </summary>
    public class ScoreCounter : MonoBehaviour, INotifyPropertyChanged
    {
        #region Public Fields & Properties

        /// <summary>
        /// Current score.
        /// </summary>
        public int Score
        {
            get { return _score; }
            set
            {
                var old = _score;
                _score = value;

                NotifyPropertyChanged("Score", old, value);
            }
        }

        /// <summary>
        /// The combo number the score is currently on.
        /// </summary>
        public int CurrentCombo
        {
            get { return _currentCombo; }
            set
            {
                var old = _currentCombo;
                _currentCombo = value;

                NotifyPropertyChanged("CurrentCombo", old, value);
            }
        }

        /// <summary>
        /// Score values for each combo number. The 0th item maps to the 1st combo.
        /// </summary>
        public int[] ComboValues { get { return _comboValues; } set { _comboValues = value; } }

        /// <summary>
        /// Game object to make copies of when showing floating scores.
        /// </summary>
        public SrLegacyScoreView FloatingScoreView { get { return _floatingScoreView; } set { _floatingScoreView = value; } }

        /// <summary>
        /// <para>
        /// Some event args may be downcast to PropertyChangedExtendedEventArgs&lt;T&gt; which contains the OldValue
        /// and NewValue fields.
        /// </para>
        /// <para>
        /// The following properties broadcast PropertyChangedExtendedEventArgs&lt;T&gt; when they are modified: 
        /// <see cref="Score"/>, <see cref="CurrentCombo"/>
        /// </para>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private & Inspector Fields

        [SerializeField]
        [FormerlySerializedAs("Score")]
        [Tooltip("Current score.")]
        private int _score;

        [SerializeField]
        [FormerlySerializedAs("Combo")]
        [Tooltip("The combo number the score is currently on.")]
        private int _currentCombo;

        [SerializeField]
        [FormerlySerializedAs("ComboValues")]
        [Tooltip("Score values for each combo number. The 0th item maps to the 1st combo.")]
        private int[] _comboValues;

        [SerializeField]
        [FormerlySerializedAs("FloatingScoreView")]
        [Tooltip("Game object to make copies of when showing floating scores.")]
        private SrLegacyScoreView _floatingScoreView;

        #endregion

        #region Public Helper Functions

        /// <summary>
        /// Adds to the current score combo.
        /// </summary>
        public void AddCombo()
        {
            AddCombo(null);
        }

        /// <summary>
        /// Adds to the current score combo using the given source.
        /// </summary>
        /// <param name="source">Source of the combo increase, used to show floating score.</param>
        public void AddCombo(Vector3? source)
        {
            var value = ComboValues[Mathf.Min(CurrentCombo++, ComboValues.Length - 1)];

            AddScore(value, source);
        }

        /// <summary>
        /// Ends the current score combo.
        /// </summary>
        public void EndCombo()
        {
            CurrentCombo = 0;
        }

        /// <summary>
        /// Adds the given value to the score.
        /// </summary>
        public void AddScore(int value)
        {
            AddScore(value, null);
        }

        /// <summary>
        /// Adds the given value to the score. If a source position is given, the counter will create a floating score there.
        /// </summary>
        public void AddScore(int value, Vector3? source)
        {
            Score += value;

            if (source != null && FloatingScoreView)
            {
                ShowFloatingScore(value, source.Value);
            }
        }

        /// <summary>
        /// Creates a floating score without actually changing the counter's current score.
        /// </summary>
        public SrLegacyScoreView ShowFloatingScore(int value, Vector3 position)
        {
            var view = Instantiate(FloatingScoreView);
            view.transform.position = position;
            view.Value = value;

            return view;
        }

        #endregion

        #region Event Functions

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyPropertyChanged<T>(string propertyName, T oldvalue, T newvalue)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new SrPropertyChangedExtendedEventArgs<T>(propertyName, oldvalue, newvalue));
        }

        #endregion

        #region Lifecycle Functions

        public void Reset()
        {
            ComboValues = new[]
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
        }

        #endregion
    }
}
