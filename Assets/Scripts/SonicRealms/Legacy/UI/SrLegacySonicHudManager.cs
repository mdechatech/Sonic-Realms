using SonicRealms.Legacy.Game;
using UnityEngine;

namespace SonicRealms.Legacy.UI
{
    /// <summary>
    /// Updates the Sonic UI with the player's information.
    /// </summary>
    public class SrLegacySonicHudManager : MonoBehaviour
    {
        public SrLegacySonicHud Hud;
        public SrLegacyGoalLevelManager Level;

        public void Reset()
        {
            Hud = FindObjectOfType<SrLegacySonicHud>();
        }

        public void Start()
        {
            if (Level == null)
            {
                enabled = false;
                return;
            }

            if (Level.RingCounter != null)
                Level.RingCounter.OnValueChange.AddListener(UpdateRings);

            if (Level.ScoreCounter != null)
                Level.ScoreCounter.PropertyChanged += (sender, e) => UpdateScore();

            if (Level.LifeCounter != null)
                Level.LifeCounter.OnValueChange.AddListener(UpdateLives);

            UpdateAll();
        }

        public void Update()
        {
            UpdateTimer();
        }

        public void UpdateAll()
        {
            if (Hud == null)
                return;

            UpdateRings();
            UpdateScore();
            UpdateTimer();
            UpdateLives();
        }

        public void UpdateRings()
        {
            if(Hud != null && Hud.Rings != null)
                Hud.Rings.Show(Level.Rings);
        }

        public void UpdateScore()
        {
            if (Hud != null && Hud.Score != null)
                Hud.Score.Value = Level.Score;
        }

        public void UpdateTimer()
        {
            if(Hud != null && Hud.Timer != null)
                Hud.Timer.Show(Level.Time);
        }

        public void UpdateLives()
        {
            if(Hud != null && Hud.LifeCounter != null)
                Hud.LifeCounter.Display(Level.Lives);
        }
    }
}
