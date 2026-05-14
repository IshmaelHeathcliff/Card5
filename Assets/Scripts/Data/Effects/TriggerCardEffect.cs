using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public class TriggerCardEffect : CardEffect
    {
        [SerializeField, LabelText("目标选择"), HideLabel, InlineProperty]
        CardTargetSelector _targetSelector = new CardTargetSelector();

        public TriggerCardEffect()
            : base(CardEffectTiming.Play)
        {
        }

        public override void Execute(BattleContext context)
        {
            if (context == null || context.BattleSystem == null) return;

            context.BattleSystem.TriggerRegisteredCardEffects(context, _targetSelector);
        }

        public override string GetDescription()
        {
            return $"激发{_targetSelector.GetDescription()}的被激发效果";
        }
    }
}
