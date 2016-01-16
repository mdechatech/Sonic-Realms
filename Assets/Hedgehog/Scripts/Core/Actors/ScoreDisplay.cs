using UnityEngine;

namespace Hedgehog.Core.Actors
{
    public abstract class ScoreDisplay : MonoBehaviour
    {
        public abstract void Display(int score);
    }
}
