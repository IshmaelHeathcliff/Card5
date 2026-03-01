using UnityEngine;
using System.Collections.Generic;
namespace Gameplay.Character.Enemy
{
    public class ClosestEnemyQuery : AbstractQuery<Transform>
    {
        Vector2 _position;
        readonly List<string> _exclude;
        public ClosestEnemyQuery(Vector2 position, List<string> exclude = null)
        {
            _position = position;
            _exclude = exclude;
        }

        protected override Transform OnDo()
        {
            return this.GetSystem<PositionQuerySystem>().QueryClosest("Enemy", _position, _exclude);
        }
    }
}
