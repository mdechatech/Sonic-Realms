using System;

namespace SonicRealms.Level
{
    [Serializable]
    public class TimeScoreBonusData
    {
        public int DefaultBonus;

        public TimeScoreBonusRange[] PossibleBonuses;

        public int GetBonus(float time)
        {
            foreach (var range in PossibleBonuses)
            {
                if (time >= range.TimeMin && time <= range.TimeMax)
                    return range.ScoreBonus;
            }

            return DefaultBonus;
        }

        public int this[float time]
        {
            get { return GetBonus(time); }
        }
    }
}
