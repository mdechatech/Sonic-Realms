using System.Linq;
using Hedgehog.Core.Actors;

namespace Hedgehog.Core.Moves
{
    public class Shield : Move
    {
        public override MoveGroup[] Groups
        {
            get { return new[] {MoveGroup.Shield}; }
        }

        public void OnHurt()
        {
            Destroy(gameObject);
            Manager.UpdateList();
        }

        public override void OnManagerAdd()
        {
            var health = Controller.GetComponent<HedgehogHealth>();
            health.OnHurt.AddListener(OnHurt);

            // Replace any previous shields
            var shield = Manager.GetMoves(MoveGroup.Shield).FirstOrDefault(move => move != this);
            if (shield != null) Destroy(shield.gameObject);
        }
    }
}
