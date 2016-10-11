using SonicRealms.Core.Actors;
using SonicRealms.Core.Triggers;
using SonicRealms.Core.Utils;
using UnityEngine;

namespace SonicRealms.Level.Effects
{
    public class AddScore : ReactiveEffect
    {
        /// <summary>
        /// Score amount to give to the player. Comes with an optional curve to change this amount based
        /// on how many times the effect has been activated.
        /// </summary>
        public ScaledCurve Amount { get { return _amount; } set { _amount = value; } }

        /// <summary>
        /// Maximum number of times this effect can be activated.
        /// </summary>
        public int MaxTimes { get { return _maxTimes; } set { _maxTimes = value; } }

        /// <summary>
        /// How many more times the effect can be activated.
        /// </summary>
        public int TimesRemaining { get { return MaxTimes - _timesActivated; } }

        /// <summary>
        /// How many times the effect has been activated.
        /// </summary>
        public int TimesActivated { get { return _timesActivated; } set { _timesActivated = value; } }

        /// <summary>
        /// A source object to pass along to the player's score counter. Can be empty.
        /// </summary>
        public Transform Source { get { return _source; } set { _source = value; } }

        [SerializeField]
        [Tooltip("Score amount to give to the player. Comes with an optional curve to change this amount " +
                 "based on how many times the effect has been activated.")]
        private ScaledCurve _amount;

        [SerializeField]
        [Tooltip("Maximum number of times this effect can be activated.")]
        private int _maxTimes;

        [Space]
        [SerializeField]
        [Tooltip("A source object to pass along to the player's score counter. Can be empty.")]
        private Transform _source;

        private int _timesActivated;

        public override void Reset()
        {
            base.Reset();

            _amount = new ScaledCurve {Curve = AnimationCurve.Linear(0, 1, 1, 1), Scale = 10};
            _maxTimes = 10;
            _source = transform;
        }

        public override void OnActivate(HedgehogController controller)
        {
            var counter = controller.GetComponent<ScoreCounter>();

            if (!counter)
                return;

            if (++TimesActivated > MaxTimes)
                return;

            counter.AddScore(
                Mathf.RoundToInt(_amount.Evaluate(TimesActivated/(float)MaxTimes)),
                _source ? _source.position : default(Vector3?));
        }
    }
}
