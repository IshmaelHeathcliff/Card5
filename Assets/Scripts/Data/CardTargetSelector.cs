using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    public enum CardTargetRange
    {
        [InspectorName("任意卡牌")]
        Any,
        [InspectorName("自身")]
        Self,
        [InspectorName("相邻牌")]
        Adjacent,
        [InspectorName("指定槽位")]
        SpecificSlots
    }

    public enum CardTargetQuantityMode
    {
        [InspectorName("全部")]
        All,
        [InspectorName("随机一个")]
        RandomOne,
        [InspectorName("随机多个")]
        RandomCount
    }

    public interface ICardTargetCandidate
    {
        int SlotIndex { get; }
        CardData Card { get; }
    }

    public readonly struct CardSlotTarget : ICardTargetCandidate
    {
        public CardSlotTarget(int slotIndex, CardData card)
        {
            SlotIndex = slotIndex;
            Card = card;
        }

        public int SlotIndex { get; }
        public CardData Card { get; }
    }

    [Serializable]
    public class CardTargetSelector
    {
        [SerializeField, LabelText("范围"), EnumToggleButtons]
        CardTargetRange _range = CardTargetRange.Any;

        [SerializeField, LabelText("指定槽位"), EnumToggleButtons, ShowIf(nameof(UsesSpecificSlots))]
        CardActivationPosition _slots = CardActivationPosition.Any;

        [SerializeField, LabelText("按类型筛选")]
        bool _useTypeFilter;

        [SerializeField, LabelText("目标类型"), EnumToggleButtons, ShowIf(nameof(UsesTypeFilter))]
        CardType _requiredType = CardType.Common;

        [SerializeField, LabelText("按标签筛选")]
        bool _useTagFilter;

        [SerializeField, LabelText("目标标签"), ShowIf(nameof(UsesTagFilter))]
        string _requiredTag;

        [SerializeField, LabelText("数量"), EnumToggleButtons]
        CardTargetQuantityMode _quantityMode = CardTargetQuantityMode.All;

        [SerializeField, LabelText("随机数量"), MinValue(1), ShowIf(nameof(UsesRandomCount))]
        int _randomCount = 1;

        bool UsesSpecificSlots => _range == CardTargetRange.SpecificSlots;
        bool UsesTypeFilter => _useTypeFilter;
        bool UsesTagFilter => _useTagFilter;
        bool UsesRandomCount => _quantityMode == CardTargetQuantityMode.RandomCount;

        public CardTargetRange Range => _range;
        public CardTargetQuantityMode QuantityMode => _quantityMode;

        public List<T> Select<T>(BattleContext context, IReadOnlyList<T> candidates)
            where T : ICardTargetCandidate
        {
            var matchedTargets = new List<T>();
            if (context == null || candidates == null) return matchedTargets;

            CardActivationPosition normalizedSlots = NormalizeSlots(_slots);
            foreach (T candidate in candidates)
            {
                if (!MatchesRange(context, candidate, normalizedSlots)) continue;
                if (!MatchesType(candidate.Card)) continue;
                if (!MatchesTag(candidate.Card)) continue;

                matchedTargets.Add(candidate);
            }

            return SelectByQuantity(matchedTargets);
        }

        public string GetDescription()
        {
            string rangeDescription = GetRangeDescription();
            string filterDescription = GetFilterDescription();
            string quantityDescription = GetQuantityDescription();
            return $"{quantityDescription}{filterDescription}{rangeDescription}";
        }

        bool MatchesRange<T>(BattleContext context, T candidate, CardActivationPosition normalizedSlots)
            where T : ICardTargetCandidate
        {
            return _range switch
            {
                CardTargetRange.Any           => true,
                CardTargetRange.Self          => candidate.SlotIndex == context.SlotIndex,
                CardTargetRange.Adjacent      => Mathf.Abs(candidate.SlotIndex - context.SlotIndex) == 1,
                CardTargetRange.SpecificSlots => IsSlotMatched(candidate.SlotIndex, normalizedSlots),
                _                             => false
            };
        }

        bool MatchesType(CardData card)
        {
            if (!_useTypeFilter) return true;
            return card != null && card.IsType(_requiredType);
        }

        bool MatchesTag(CardData card)
        {
            if (!_useTagFilter) return true;
            return card != null && card.HasTag(_requiredTag);
        }

        List<T> SelectByQuantity<T>(List<T> matchedTargets)
        {
            if (_quantityMode == CardTargetQuantityMode.All || matchedTargets.Count <= 1)
                return matchedTargets;

            int count = _quantityMode == CardTargetQuantityMode.RandomOne
                ? 1
                : Mathf.Clamp(_randomCount, 1, matchedTargets.Count);

            var selectedTargets = new List<T>(count);
            var remainingTargets = new List<T>(matchedTargets);
            for (int i = 0; i < count; i++)
            {
                int index = UnityEngine.Random.Range(0, remainingTargets.Count);
                selectedTargets.Add(remainingTargets[index]);
                remainingTargets.RemoveAt(index);
            }

            return selectedTargets;
        }

        string GetRangeDescription()
        {
            return _range switch
            {
                CardTargetRange.Any           => "任意卡牌",
                CardTargetRange.Self          => "自身",
                CardTargetRange.Adjacent      => "相邻牌",
                CardTargetRange.SpecificSlots => GetSlotDescription(),
                _                             => "目标卡牌"
            };
        }

        string GetFilterDescription()
        {
            string typeDescription = _useTypeFilter ? $"{_requiredType}" : string.Empty;
            string tagDescription = _useTagFilter && !string.IsNullOrWhiteSpace(_requiredTag)
                ? $"标签「{_requiredTag.Trim()}」"
                : string.Empty;

            if (string.IsNullOrEmpty(typeDescription)) return tagDescription;
            if (string.IsNullOrEmpty(tagDescription)) return typeDescription;
            return $"{typeDescription}且{tagDescription}";
        }

        string GetQuantityDescription()
        {
            return _quantityMode switch
            {
                CardTargetQuantityMode.RandomOne   => "随机 1 个",
                CardTargetQuantityMode.RandomCount => $"随机 {Mathf.Max(1, _randomCount)} 个",
                _                                  => "全部"
            };
        }

        string GetSlotDescription()
        {
            CardActivationPosition normalizedSlots = NormalizeSlots(_slots);
            if ((normalizedSlots & CardActivationPosition.Any) == CardActivationPosition.Any)
                return "任意槽位";
            if (normalizedSlots == CardActivationPosition.OddPositions)
                return "奇数槽位";
            if (normalizedSlots == CardActivationPosition.EvenPositions)
                return "偶数槽位";

            var slotNames = new List<string>();
            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                if (IsSlotMatched(i, normalizedSlots))
                    slotNames.Add($"{i + 1}");
            }

            return slotNames.Count > 0 ? $"{string.Join("、", slotNames)}号槽位" : "任意槽位";
        }

        static bool IsSlotMatched(int slotIndex, CardActivationPosition slots)
        {
            if (slotIndex < 0 || slotIndex >= BattleModel.SlotCount) return false;

            CardActivationPosition slot = (CardActivationPosition)(1 << slotIndex);
            return (slots & slot) != 0;
        }

        static CardActivationPosition NormalizeSlots(CardActivationPosition slots)
        {
            CardActivationPosition normalized = slots & CardActivationPosition.Any;
            return normalized == CardActivationPosition.None ? CardActivationPosition.Any : normalized;
        }
    }
}
