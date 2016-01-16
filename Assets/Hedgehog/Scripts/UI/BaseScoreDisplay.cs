using UnityEngine;

namespace Hedgehog.UI
{
    public abstract class BaseScoreDisplay : MonoBehaviour
    {
        public abstract void Display(int score);
    }
}
