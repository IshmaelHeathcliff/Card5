using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    public enum SlotLinearDirection
    {
        [InspectorName("递增")]
        Increase,
        [InspectorName("递减")]
        Decrease
    }

    [Serializable]
    public class BoostSlotCardEffect : CardEffect
    {
        [SerializeField, LabelText("目标选择"), HideLabel, InlineProperty]
        CardTargetSelector _targetSelector = new CardTargetSelector();

        [SerializeField, LabelText("提升方式")]
        CardEffectBoostMode _boostMode = CardEffectBoostMode.AddFlat;

        [SerializeField, LabelText("基础伤害增加"), MinValue(0), ShowIf(nameof(UsesStaticFlatAmount))]
        int _flatAmount = 1;

        [SerializeField, LabelText("伤害百分比增减"), SuffixLabel("%"), ShowIf(nameof(UsesStaticPercentAmount))]
        float _percentAmount = 50f;

        [SerializeField, LabelText("伤害总增倍率"), MinValue(0), ShowIf(nameof(UsesStaticMultiplier))]
        float _multiplier = 2f;

        [SerializeField, LabelText("固定值增加"), MinValue(0), ShowIf(nameof(UsesStaticFixedAmount))]
        int _fixedAmount = 1;

        [SerializeField, LabelText("按所在槽位线性变化")]
        bool _useSlotLinearValue;

        [SerializeField, LabelText("基础数值/倍率"), ShowIf(nameof(UsesSlotLinearValue))]
        float _linearBaseValue = 1f;

        [SerializeField, LabelText("每槽变化"), MinValue(0), ShowIf(nameof(UsesSlotLinearValue))]
        float _linearStepValue = 0.25f;

        [SerializeField, LabelText("变化方向"), EnumToggleButtons, ShowIf(nameof(UsesSlotLinearValue))]
        SlotLinearDirection _linearDirection = SlotLinearDirection.Increase;

        bool UsesFlatAmount => _boostMode == CardEffectBoostMode.AddFlat;
        bool UsesPercentAmount => _boostMode == CardEffectBoostMode.AddPercent;
        bool UsesMultiplier => _boostMode == CardEffectBoostMode.Multiply;
        bool UsesFixedAmount => _boostMode == CardEffectBoostMode.AddFixed;
        bool UsesSlotLinearValue => _useSlotLinearValue;
        bool UsesStaticFlatAmount => UsesFlatAmount && !UsesSlotLinearValue;
        bool UsesStaticPercentAmount => UsesPercentAmount && !UsesSlotLinearValue;
        bool UsesStaticMultiplier => UsesMultiplier && !UsesSlotLinearValue;
        bool UsesStaticFixedAmount => UsesFixedAmount && !UsesSlotLinearValue;

        public BoostSlotCardEffect()
            : base(CardEffectTiming.PlayStart)
        {
        }

        public override void Execute(BattleContext context)
        {
            if (context == null || context.BattleSystem == null) return;

            CardEffectBoost boost = new CardEffectBoost(_boostMode, GetBoostValue(context.SlotIndex));
            List<CardSlotTarget> targets = SelectTargets(context);
            Debug.Log($"[CardEffect] 增伤目标选择 | 来源卡牌={FormatCard(context.CurrentCard)} | 来源槽位={FormatSlot(context.SlotIndex)} | 选择器={_targetSelector.GetDescription()} | 目标数={targets.Count} | 增伤={GetBoostDebugDescription(boost)}");

            foreach (CardSlotTarget target in targets)
            {
                Debug.Log($"[CardEffect] 增伤添加 | 来源卡牌={FormatCard(context.CurrentCard)} | 目标卡牌={FormatCard(target.Card)} | 目标槽位={FormatSlot(target.SlotIndex)} | 增伤={GetBoostDebugDescription(boost)}");
                context.BattleSystem.AddCardEffectBoost(target.SlotIndex, boost);
            }
        }

        public override string GetDescription()
        {
            return $"使{_targetSelector.GetDescription()}本轮出牌阶段伤害{GetBoostDescription()}";
        }

        List<CardSlotTarget> SelectTargets(BattleContext context)
        {
            var candidates = new List<CardSlotTarget>(BattleModel.SlotCount);
            if (context.BattleModel == null) return candidates;

            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                candidates.Add(new CardSlotTarget(i, context.BattleModel.PlaySlots[i]));
            }

            return _targetSelector.Select(context, candidates);
        }

        float GetBoostValue(int slotIndex)
        {
            if (_useSlotLinearValue)
                return GetLinearBoostValue(slotIndex);

            float value = _boostMode switch
            {
                CardEffectBoostMode.AddFlat    => _flatAmount,
                CardEffectBoostMode.AddPercent => _percentAmount,
                CardEffectBoostMode.Multiply   => _multiplier,
                CardEffectBoostMode.AddFixed   => _fixedAmount,
                _                              => 0f
            };

            return value;
        }

        float GetLinearBoostValue(int slotIndex)
        {
            int clampedSlotIndex = Mathf.Clamp(slotIndex, 0, BattleModel.SlotCount - 1);
            int stepCount = _linearDirection == SlotLinearDirection.Increase
                ? clampedSlotIndex
                : BattleModel.SlotCount - 1 - clampedSlotIndex;

            float value = _linearBaseValue + _linearStepValue * stepCount;
            return _boostMode == CardEffectBoostMode.AddPercent ? value : Mathf.Max(0f, value);
        }

        string GetBoostDescription()
        {
            if (_useSlotLinearValue)
                return _boostMode == CardEffectBoostMode.Multiply
                    ? $"按所在槽位{GetLinearDirectionDescription()}，从 {_linearBaseValue:0.##} 倍起每槽变化 {_linearStepValue:0.##}"
                    : $"按所在槽位{GetLinearDirectionDescription()}，从 {_linearBaseValue:0.##} 起每槽变化 {_linearStepValue:0.##}";

            return _boostMode switch
            {
                CardEffectBoostMode.AddFlat    => $"基础伤害增加 {_flatAmount}",
                CardEffectBoostMode.AddPercent => $"伤害增减 {_percentAmount:0.#}%",
                CardEffectBoostMode.Multiply   => $"伤害总增 {_multiplier:0.##} 倍",
                CardEffectBoostMode.AddFixed   => $"固定值增加 {_fixedAmount}",
                _                              => "提高"
            };
        }

        string GetLinearDirectionDescription()
        {
            return _linearDirection == SlotLinearDirection.Increase ? "递增" : "递减";
        }

        static string GetBoostDebugDescription(CardEffectBoost boost)
        {
            return $"{boost.Mode}:{boost.Value:0.##}";
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
    }
}
