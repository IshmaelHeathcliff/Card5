using UnityEngine;

namespace Gameplay.Character.Player
{
    public class PlayerPositionQuery : AbstractQuery<Vector2>
    {
        protected override Vector2 OnDo()
        {
            return this.GetModel<PlayersModel>().Current.Position;
        }
    }

    public class PlayerDirectionQuery : AbstractQuery<Vector2>
    {
        protected override Vector2 OnDo()
        {
            return this.GetModel<PlayersModel>().Current.Direction;
        }
    }
}
