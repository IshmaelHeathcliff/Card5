using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "BoostSlotCardEffect", menuName = "Card5/Effects/Boost Slot Card")]
    public class BoostSlotCardEffectSO : CardEffectSO
    {
        [SerializeField, LabelText("目标槽位"), EnumToggleButtons]
        CardActivationPosition _targetSlots = CardActivationPosition.Any;

        [SerializeField, LabelText("提升方式")]
        CardEffectBoostMode _boostMode = CardEffectBoostMode.AddFlat;

        [SerializeField, LabelText("固定增加"), MinValue(0), ShowIf(nameof(UsesFlatAmount))]
        int _flatAmount = 1;

        [SerializeField, LabelText("百分比增加"), MinValue(0), SuffixLabel("%"), ShowIf(nameof(UsesPercentAmount))]
        float _percentAmount = 50f;

        [SerializeField, LabelText("倍率"), MinValue(0), ShowIf(nameof(UsesMultiplier))]
        float _multiplier = 2f;

        bool UsesFlatAmount => _boostMode == CardEffectBoostMode.AddFlat;
        bool UsesPercentAmount => _boostMode == CardEffectBoostMode.AddPercent;
        bool UsesMultiplier => _boostMode == CardEffectBoostMode.Multiply;

        public override void Execute(BattleContext context)
        {
            if (context == null || context.BattleSystem == null) return;

            CardEffectBoost boost = new CardEffectBoost(_boostMode, GetBoostValue());
            CardActivationPosition targetSlots = NormalizeTargetSlots(_targetSlots);

            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                CardActivationPosition position = (CardActivationPosition)(1 << i);
                if ((targetSlots & position) == 0) continue;

                context.BattleSystem.AddCardEffectBoost(i, boost);
            }
        }

        public override string GetDescription()
        {
            return $"使{GetTargetSlotDescription()}本轮后续卡牌效果{GetBoostDescription()}";
        }

        void OnValidate()
        {
            _targetSlots = NormalizeTargetSlots(_targetSlots);
        }

        float GetBoostValue()
        {
            return _boostMode switch
            {
                CardEffectBoostMode.AddFlat    => _flatAmount,
                CardEffectBoostMode.AddPercent => _percentAmount,
                CardEffectBoostMode.Multiply   => _multiplier,
                _                              => 0f
            };
        }

        string GetBoostDescription()
        {
            return _boostMode switch
            {
                CardEffectBoostMode.AddFlat    => $"固定增加 {_flatAmount}",
                CardEffectBoostMode.AddPercent => $"提高 {_percentAmount:0.#}%",
                CardEffectBoostMode.Multiply   => $"变为 {_multiplier:0.##} 倍",
                _                              => "提高"
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
