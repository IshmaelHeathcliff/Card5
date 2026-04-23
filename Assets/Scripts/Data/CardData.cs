using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [Flags]
    public enum CardActivationPosition
    {
        [InspectorName("无")]
        None = 0,
        [InspectorName("1号位")]
        Position1 = 1 << 0,
        [InspectorName("2号位")]
        Position2 = 1 << 1,
        [InspectorName("3号位")]
        Position3 = 1 << 2,
        [InspectorName("4号位")]
        Position4 = 1 << 3,
        [InspectorName("5号位")]
        Position5 = 1 << 4,
        [InspectorName("奇数位")]
        OddPositions = Position1 | Position3 | Position5,
        [InspectorName("偶数位")]
        EvenPositions = Position2 | Position4,
        [InspectorName("任意位置")]
        Any = Position1 | Position2 | Position3 | Position4 | Position5
    }

    [CreateAssetMenu(fileName = "NewCard", menuName = "Card5/Card")]
    public class CardData : SerializedScriptableObject
    {
        [SerializeField] string _cardId;
        [SerializeField] string _cardName;
        [SerializeField, TextArea] string _description;
        [SerializeField, MinValue(0)] int _energyCost;
        [SerializeField, LabelText("生效位置"), EnumToggleButtons] CardActivationPosition _activationPositions = CardActivationPosition.Any;
        [SerializeField] Sprite _artwork;
        [SerializeField] List<CardEffectSO> _effects = new List<CardEffectSO>();

        public string CardId => _cardId;
        public string CardName => _cardName;
        public string Description => _description;
        public int EnergyCost => _energyCost;
        public CardActivationPosition ActivationPositions => NormalizeActivationPositions(_activationPositions);
        [ShowInInspector, ReadOnly, LabelText("生效位置说明")]
        public string ActivationPositionDescription => GetActivationPositionDescription();
        public Sprite Artwork => _artwork;
        public IReadOnlyList<CardEffectSO> Effects => _effects;

        public bool CanActivateAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 5) return false;
            CardActivationPosition position = (CardActivationPosition)(1 << slotIndex);
            return (ActivationPositions & position) != 0;
        }

        public string GetActivationPositionDescription()
        {
            CardActivationPosition positions = ActivationPositions;
            if ((positions & CardActivationPosition.Any) == CardActivationPosition.Any)
                return "任意位置";
            if (positions == CardActivationPosition.OddPositions)
                return "奇数位";
            if (positions == CardActivationPosition.EvenPositions)
                return "偶数位";

            var builder = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                CardActivationPosition position = (CardActivationPosition)(1 << i);
                if ((positions & position) == 0) continue;

                if (builder.Length > 0)
                    builder.Append("、");
                builder.Append(i + 1);
            }

            return builder.Length > 0 ? $"{builder}号位" : "任意位置";
        }

        public string GetFullDescription()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"生效位置：{GetActivationPositionDescription()}");

            if (!string.IsNullOrWhiteSpace(_description))
                builder.AppendLine(_description.Trim());

            foreach (CardEffectSO effect in _effects)
            {
                if (effect == null) continue;

                string effectDescription = effect.GetDescription();
                if (!string.IsNullOrWhiteSpace(effectDescription))
                    builder.AppendLine(effectDescription);
            }

            return builder.ToString().TrimEnd();
        }

        [HorizontalGroup("PositionPreset")]
        [Button("任意位置")]
        void SetAnyPositions()
        {
            _activationPositions = CardActivationPosition.Any;
        }

        [HorizontalGroup("PositionPreset")]
        [Button("奇数位")]
        void SetOddPositions()
        {
            _activationPositions = CardActivationPosition.OddPositions;
        }

        [HorizontalGroup("PositionPreset")]
        [Button("偶数位")]
        void SetEvenPositions()
        {
            _activationPositions = CardActivationPosition.EvenPositions;
        }

        void OnValidate()
        {
            if (string.IsNullOrEmpty(_cardId))
                _cardId = name;
            if (_activationPositions == CardActivationPosition.None)
                _activationPositions = CardActivationPosition.Any;
        }

        static CardActivationPosition NormalizeActivationPositions(CardActivationPosition positions)
        {
            CardActivationPosition normalized = positions & CardActivationPosition.Any;
            return normalized == CardActivationPosition.None ? CardActivationPosition.Any : normalized;
        }
    }
}
