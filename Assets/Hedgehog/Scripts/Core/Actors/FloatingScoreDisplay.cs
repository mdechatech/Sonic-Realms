using UnityEngine;

namespace Hedgehog.Core.Actors
{
    public class FloatingScoreDisplay : ScoreDisplay
    {
        public TextMesh TextMesh;

        public void Reset()
        {
            TextMesh = GetComponentInChildren<TextMesh>();
        }

        public override void Display(int score)
        {
            if(TextMesh != null) TextMesh.text = score.ToString();
        }
    }
}
