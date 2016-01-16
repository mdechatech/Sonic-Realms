using Hedgehog.Core.Actors;
using Hedgehog.Level;
using UnityEngine;

namespace Hedgehog.UI
{
    public class SonicUIManager : MonoBehaviour
    {
        public SonicUI UI;

        public RingCollector RingCounter;
        public ScoreCounter ScoreCounter;
        public ZoneTimer ZoneTimer;

        public void Reset()
        {
            UI = FindObjectOfType<SonicUI>();

            RingCounter = FindObjectOfType<RingCollector>();
            ScoreCounter = FindObjectOfType<ScoreCounter>();
            ZoneTimer = FindObjectOfType<ZoneTimer>();
        }

        public void Start()
        {
            if(RingCounter) RingCounter.OnAmountChange.AddListener(UpdateRings);
            if(ScoreCounter) ScoreCounter.OnValueChanged.AddListener(UpdateScore);
        }

        public void Update()
        {
            UpdateTimer();
        }

        public void UpdateRings()
        {
            UI.Rings.Display(RingCounter.Rings);
        }

        public void UpdateScore()
        {
            UI.Score.Display(ScoreCounter.Score);
        }

        public void UpdateTimer()
        {
            UI.Timer.Display(ZoneTimer.Time);
        }
    }
}
