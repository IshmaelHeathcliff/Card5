using System.Collections.Generic;
using System.Linq;
using Gameplay.Character.Enemy;
using Gameplay.Character.Player;
using UnityEngine;

namespace Gameplay.Character
{
    public class PositionQuerySystem : AbstractSystem
    {
        Dictionary<string, Transform> GetTransforms(string tag)
        {
            return tag switch
            {
                "Player" => this.GetModel<PlayersModel>().GetTransforms(),
                "Enemy" => this.GetModel<EnemiesModel>().GetTransforms(),
                _ => new Dictionary<string, Transform>(),
            };
        }

        public List<Transform> Query(string tag, Vector2 position, float radius, List<string> exclude = null)
        {
            return GetTransforms(tag)
                .Where(pair => exclude == null || !exclude.Contains(pair.Key))
                .Where(pair => Vector2.Distance(pair.Value.position, position) <= radius)
                .Select(pair => pair.Value)
                .ToList();
        }

        public Transform QueryClosest(string tag, Vector2 position, float radius, List<string> exclude = null)
        {
            float radiusSquared = radius * radius;
            return GetTransforms(tag)
                .Where(pair => exclude == null || !exclude.Contains(pair.Key))
                .Where(pair => ((Vector2)pair.Value.position - position).sqrMagnitude <= radiusSquared)
                .OrderBy(pair => ((Vector2)pair.Value.position - position).sqrMagnitude)
                .Select(pair => pair.Value)
                .FirstOrDefault();
        }

        public Transform QueryClosest(string tag, Vector2 position, List<string> exclude = null)
        {
            return GetTransforms(tag)
                .Where(pair => exclude == null || !exclude.Contains(pair.Key))
                .OrderBy(pair => ((Vector2)pair.Value.position - position).sqrMagnitude)
                .Select(pair => pair.Value)
                .FirstOrDefault();
        }


        protected override void OnInit()
        {
        }
    }
}
