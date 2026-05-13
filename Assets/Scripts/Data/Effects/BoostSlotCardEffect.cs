using System;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    public enum CardEffectBoostTargetScope
    {
        [InspectorName("指定槽位")]
        SpecificSlots,
        [InspectorName("自身")]
        Self,
        [InspectorName("相邻牌")]
        Adjacent
    }

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
        [SerializeField, LabelText("作用范围"), EnumToggleButtons]
        CardEffectBoostTargetScope _targetScope = CardEffectBoostTargetScope.SpecificSlots;

        [SerializeField, LabelText("目标槽位"), EnumToggleButtons, ShowIf(nameof(UsesSpecificSlots))]
        CardActivationPosition _targetSlots = CardActivationPosition.Any;

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

        bool UsesSpecificSlots => _targetScope == CardEffectBoostTargetScope.SpecificSlots;
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
            AddBoostToTargets(context, boost);
        }

        public override string GetDescription()
        {
            return $"使{GetTargetDescription()}本轮出牌阶段伤害{GetBoostDescription()}";
        }

        void AddBoostToTargets(BattleContext context, CardEffectBoost boost)
        {
            switch (_targetScope)
            {
                case CardEffectBoostTargetScope.Self:
                    AddBoostToSlot(context, context.SlotIndex, boost);
                    break;
                case CardEffectBoostTargetScope.Adjacent:
                    AddBoostToSlot(context, context.SlotIndex - 1, boost);
                    AddBoostToSlot(context, context.SlotIndex + 1, boost);
                    break;
                case CardEffectBoostTargetScope.SpecificSlots:
                    AddBoostToSpecificSlots(context, boost);
                    break;
            }
        }

        void AddBoostToSpecificSlots(BattleContext context, CardEffectBoost boost)
        {
            CardActivationPosition targetSlots = NormalizeTargetSlots(_targetSlots);
            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                CardActivationPosition position = (CardActivationPosition)(1 << i);
                if ((targetSlots & position) == 0) continue;

                AddBoostToSlot(context, i, boost);
            }
        }

        void AddBoostToSlot(BattleContext context, int slotIndex, CardEffectBoost boost)
        {
            if (slotIndex < 0 || slotIndex >= BattleModel.SlotCount) return;

            context.BattleSystem.AddCardEffectBoost(slotIndex, boost);
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

        string GetTargetDescription()
        {
            return _targetScope switch
            {
                CardEffectBoostTargetScope.Self     => "自身",
                CardEffectBoostTargetScope.Adjacent => "相邻牌",
                _                                   => GetTargetSlotDescription()
            };
        }

        string GetTargetSlotDescription()
        {
            CardActivationPosition targetSlots = NormalizeTargetSlots(_targetSlots);
            if ((targetSlots & CardActivationPosition.Any) == CardActivationPosition.Any)
                return "任意槽位";
            if (targetSlots == CardActivationPosition.OddPositions)
                return "奇数槽位";
            if (targetSlots == CardActivationPosition.EvenPositions)
                return "偶数槽位";

            var builder = new StringBuilder();
            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                CardActivationPosition position = (CardActivationPosition)(1 << i);
                if ((targetSlots & position) == 0) continue;

                if (builder.Length > 0)
                    builder.Append("、");
                builder.Append(i + 1);
            }

            return builder.Length > 0 ? $"{builder}号槽位" : "任意槽位";
        }

        static CardActivationPosition NormalizeTargetSlots(CardActivationPosition targetSlots)
        {
            CardActivationPosition normalized = targetSlots & CardActivationPosition.Any;
            return normalized == CardActivationPosition.None ? CardActivationPosition.Any : normalized;
        }
    }
}
