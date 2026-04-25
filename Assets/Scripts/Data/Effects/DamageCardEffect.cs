using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public class DamageCardEffect : CardEffect
    {
        [SerializeField, LabelText("伤害数值"), MinValue(1)] int _damage = 1;

        public int Damage => _damage;

        public override void Execute(BattleContext context)
        {
            context.DealDamage(_damage);
        }

        public override string GetDescription()
        {
            return $"对敌人造成 {_damage} 点伤害";
        }
    }
}
