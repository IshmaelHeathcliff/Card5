using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public class DamageCardEffect : CardEffect
    {
        [SerializeField, MinValue(1)] int _damage = 1;
        [SerializeField] DamageTarget _target = DamageTarget.Enemy;

        public int Damage => _damage;
        public DamageTarget Target => _target;

        public override void Execute(BattleContext context)
        {
            context.DealDamage(_damage, _target);
        }

        public override string GetDescription()
        {
            string targetStr = _target == DamageTarget.Enemy ? "敌人" : "玩家";
            return $"对{targetStr}造成 {_damage} 点伤害";
        }
    }

    public enum DamageTarget
    {
        Enemy,
        Player
    }
}
