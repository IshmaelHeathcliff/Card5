using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public class HealCardEffect : CardEffect
    {
        [SerializeField, LabelText("治疗数值"), MinValue(1)] int _healAmount = 1;
        [SerializeField, LabelText("目标")] HealTarget _target = HealTarget.Player;

        public int HealAmount => _healAmount;
        public HealTarget Target => _target;

        public override void Execute(BattleContext context)
        {
            context.ApplyHeal(_healAmount, _target);
        }

        public override string GetDescription()
        {
            string targetStr = _target == HealTarget.Player ? "玩家" : "敌人";
            return $"恢复{targetStr} {_healAmount} 点生命值";
        }
    }

    public enum HealTarget
    {
        [InspectorName("玩家")]
        Player,
        [InspectorName("敌人")]
        Enemy
    }
}
