using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public class DamageCardEffect : CardEffect
    {
        [SerializeField, LabelText("使用随机范围")] bool _useRandomRange;
        [SerializeField, LabelText("伤害数值"), MinValue(1), HideIf(nameof(UseRandomRange))] int _damage = 1;
        [SerializeField, LabelText("伤害下限"), MinValue(1), ShowIf(nameof(UseRandomRange))] int _minDamage = 1;
        [SerializeField, LabelText("伤害上限"), MinValue(1), ShowIf(nameof(UseRandomRange))] int _maxDamage = 3;

        public int Damage => _damage;
        public int MinDamage => GetMinDamage();
        public int MaxDamage => GetMaxDamage();
        public bool UseRandomRange => _useRandomRange;

        public override void Execute(BattleContext context)
        {
            context.DealDamage(GetDamage());
        }

        public override string GetDescription()
        {
            if (_useRandomRange)
                return $"对敌人造成 {MinDamage}-{MaxDamage} 点随机伤害";

            return $"对敌人造成 {_damage} 点伤害";
        }

        int GetDamage()
        {
            if (!_useRandomRange) return _damage;

            return UnityEngine.Random.Range(MinDamage, MaxDamage + 1);
        }

        int GetMinDamage()
        {
            return Mathf.Max(1, Mathf.Min(_minDamage, _maxDamage));
        }

        int GetMaxDamage()
        {
            return Mathf.Max(1, Mathf.Max(_minDamage, _maxDamage));
        }
    }
}
