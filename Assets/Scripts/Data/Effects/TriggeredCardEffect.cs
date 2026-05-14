using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public class TriggeredCardEffect : CardEffect
    {
        [OdinSerialize, LabelText("被激发时效果")]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true, DraggableItems = true)]
        [PolymorphicDrawerSettings(ShowBaseType = false)]
        List<CardEffect> _effects = new List<CardEffect>();

        public TriggeredCardEffect()
            : base(CardEffectTiming.PlayStart)
        {
        }

        public override void Execute(BattleContext context)
        {
            if (context == null || context.BattleSystem == null) return;

            context.BattleSystem.RegisterTriggeredCardEffect(context, this);
        }

        public override string GetDescription()
        {
            return $"被激发时，执行 {_effects.Count} 个效果";
        }

        internal void Trigger(BattleContext context)
        {
            if (context == null || context.IsDisposed) return;

            CardEffectTiming previousTiming = context.CurrentTiming;
            bool previousUseCardEffectBoost = context.UseCardEffectBoost;
            context.SetUseCardEffectBoost(true);

            try
            {
                foreach (CardEffect effect in _effects)
                {
                    if (effect == null) continue;

                    int damageBefore = context.DamageDealtThisCard;
                    context.CurrentTiming = effect.Timing;
                    Debug.Log($"[CardEffect] 被激发内部效果开始 | 卡牌={FormatCard(context.CurrentCard)} | 槽位={FormatSlot(context.SlotIndex)} | 效果={effect.GetType().Name} | 时机={effect.Timing} | 描述={GetEffectDescription(effect)}");
                    effect.Execute(context);
                    int damageDelta = context.DamageDealtThisCard - damageBefore;
                    Debug.Log($"[CardEffect] 被激发内部效果结束 | 卡牌={FormatCard(context.CurrentCard)} | 槽位={FormatSlot(context.SlotIndex)} | 效果={effect.GetType().Name} | 本效果伤害={damageDelta} | 本卡累计伤害={context.DamageDealtThisCard}");

                    if (context.BattleModel != null && context.BattleModel.IsBattleOver) break;
                    if (context.BattleModel != null && context.BattleModel.IsCurrentMonsterDefeated) break;
                }
            }
            finally
            {
                context.SetUseCardEffectBoost(previousUseCardEffectBoost);
                context.CurrentTiming = previousTiming;
            }
        }

        static string FormatCard(CardData card)
        {
            if (card == null) return "空";

            string cardName = string.IsNullOrWhiteSpace(card.CardName) ? card.name : card.CardName;
            return $"{cardName}(ID:{card.CardId}, 类型:{card.Type})";
        }

        static string FormatSlot(int slotIndex)
        {
            return slotIndex >= 0 ? $"{slotIndex + 1}号位" : "无槽位";
        }

        static string GetEffectDescription(CardEffect effect)
        {
            if (effect == null) return string.Empty;

            string description = effect.GetDescription();
            return string.IsNullOrWhiteSpace(description) ? "无描述" : description;
        }
    }
}
