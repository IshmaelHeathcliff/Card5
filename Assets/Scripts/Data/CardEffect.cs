using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    public enum CardEffectTiming
    {
        [InspectorName("出牌阶段")]
        Play = 0,
        [InspectorName("放牌阶段")]
        Placement = 1,
        [InspectorName("出牌开始阶段")]
        PlayStart = 2,
        [InspectorName("出牌结束阶段")]
        PlayEnd = 3
    }

    [Serializable]
    public abstract class CardEffect
    {
        [SerializeField, LabelText("生效时机"), EnumToggleButtons] CardEffectTiming _timing;
        [SerializeField, LabelText("补充描述"), TextArea(2, 4)] string _description;

        public CardEffectTiming Timing => _timing;
        public string Description => _description;

        protected CardEffect(CardEffectTiming timing = CardEffectTiming.Play)
        {
            _timing = timing;
        }

        public abstract void Execute(BattleContext context);

        public virtual void Cancel(BattleContext context)
        {
        }

        public virtual string GetDescription() => _description;
    }
}
