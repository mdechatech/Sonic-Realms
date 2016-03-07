using System.Collections;
using SonicRealms.Level;
using UnityEngine;

namespace SonicRealms.UI
{
    public class ActClearScreen : Transition
    {
        [Space]
        public ScoreView TimeBonusView;
        public ScoreView RingBonusView;
        public ScoreView TotalBonusView;

        /// <summary>
        /// How quickly the score bonus is added to the player's score counter, in points per second.
        /// </summary>
        [Space, Tooltip("How quickly the score bonus is added to the player's score counter, in points per second.")]
        public float ScoreAddRate;

        /// <summary>
        /// How many seconds to wait after the score bonus has been added until it is done playing.
        /// </summary>
        [Tooltip("How many seconds to wait after the score bonus has been added until it is done playing.")]
        public float SecondsAfterScoreAdd;
    
        /// <summary>
        /// Bonus score from level time.
        /// </summary>
        [Space, Tooltip("Bonus score from level time.")]
        public TimeScoreBonusData TimeBonus;

        /// <summary>
        /// Bonus score per ring.
        /// </summary>
        [Tooltip("Bonus score per ring.")]
        public float RingBonus;

        [Space]
        public AudioSource FinishedSound;
        public AudioSource DripSound;
        public float DripSoundPlayRate;

        protected int TotalBonusDrip;
        protected int TimeBonusDrip;
        protected int RingBonusDrip;

        protected GoalLevelManager Level;

        protected Coroutine DripSoundCoroutine;

        public override void Reset()
        {
            base.Reset();

            ScoreAddRate = 6000f;
            SecondsAfterScoreAdd = 3f;
        }

        public override void Enter()
        {
            base.Enter();

            Level = GameManager.Instance.Level as GoalLevelManager;
            if (Level == null) return;

            TimeBonusDrip = TimeBonus[Level.TimeSeconds];
            RingBonusDrip = Level.Rings*(int)RingBonus;
            TotalBonusDrip = 0;

            UpdateBonusDisplays();
        }

        public override void Exit()
        {
            base.Exit();
            if (DripSound != null) DripSoundCoroutine = StartCoroutine(PlayDripSound());
        }

        public override void OnPlayingUpdate()
        {
            if (State != TransitionState.Exit) return;
            if (TimeBonusDrip == 0 && RingBonusDrip == 0) return;

            var timeBonusDelta = Mathf.Min(TimeBonusDrip, (int)(ScoreAddRate * Time.deltaTime));
            TimeBonusDrip -= timeBonusDelta;

            var ringBonusDelta = Mathf.Min(RingBonusDrip, (int)(ScoreAddRate * Time.deltaTime));
            RingBonusDrip -= ringBonusDelta;

            TotalBonusDrip += timeBonusDelta + ringBonusDelta;
            Level.Score += timeBonusDelta + ringBonusDelta;

            UpdateBonusDisplays();

            if (TimeBonusDrip == 0 && RingBonusDrip == 0) DripComplete();
        }

        public void DripComplete()
        {
            StartCoroutine(WaitAfterDripComplete());
            Level.Score += TimeBonusDrip + RingBonusDrip;
            TimeBonusDrip = RingBonusDrip = 0;

            if (DripSound != null) StopDripSound();
            if (FinishedSound != null) FinishedSound.Play();
        }

        public void UpdateBonusDisplays()
        {
            TotalBonusView.Show(TotalBonusDrip);
            RingBonusView.Show(RingBonusDrip);
            TimeBonusView.Show(TimeBonusDrip);
        }

        protected IEnumerator WaitAfterDripComplete()
        {
            yield return new WaitForSeconds(SecondsAfterScoreAdd);
            ExitComplete();
        }

        protected IEnumerator PlayDripSound()
        {
            if (DripSound == null) yield break;

            while (true)
            {
                DripSound.Play();
                yield return new WaitForSeconds(1f/DripSoundPlayRate);
            }
        }

        protected void StopDripSound()
        {
            if (DripSound == null) return;
            if (DripSoundCoroutine == null) return;

            StopCoroutine(DripSoundCoroutine);
            DripSound.Stop();
        }
    }
}
