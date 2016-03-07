using System;

namespace SonicRealms.Level
{
    [Serializable]
    public class SaveData
    {
        public string Name;

        public string Character;
        public int Lives;

        public string Level;
        public string Checkpoint;
        public int Score;
        public int Rings;
        public float Time;

        public override string ToString()
        {
            return
                string.Format(
                    "Name: {0}, Character: {1}, Lives: {2}, Level: {3}, Checkpoint: {4}, Score: {5}, Rings: {6}, Time: {7}",
                    Name, Character, Lives, Level, Checkpoint, Score, Rings, Time);
        }
    }
}
