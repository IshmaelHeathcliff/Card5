using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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

    [Flags]
    public enum CardTag
    {
        [InspectorName("无")]
        None = 0,
        [InspectorName("仪式")]
        Ritual = 1 << 0,
        [InspectorName("法术")]
        Spell = 1 << 1,
        [InspectorName("战意")]
        BattleWill = 1 << 2
    }

    [HideMonoScript]
    [CreateAssetMenu(fileName = "NewCard", menuName = "Card5/Card")]
    public class CardData : SerializedScriptableObject
    {
        [BoxGroup("基础信息文本"), SerializeField, LabelText("卡牌ID")] string _cardId;
        [BoxGroup("基础信息文本"), SerializeField, LabelText("卡牌名称")] string _cardName;
        [BoxGroup("基础信息文本"), SerializeField, LabelText("描述"), TextArea(3, 6)] string _description;
        [BoxGroup("基础信息文本"), SerializeField, LabelText("能量消耗"), MinValue(0)] int _energyCost;
        [BoxGroup("基础信息配置"), SerializeField, LabelText("卡牌标签"), EnumToggleButtons] CardTag _tags = CardTag.None;
        [BoxGroup("基础信息配置"), SerializeField, LabelText("生效位置"), EnumToggleButtons] CardActivationPosition _activationPositions = CardActivationPosition.Any;
        [BoxGroup("基础信息视觉"), SerializeField, LabelText("卡面图片"), PreviewField(100, ObjectFieldAlignment.Center)] Sprite _artwork;
        [BoxGroup("效果配置")]
        [OdinSerialize, LabelText("卡牌效果"), ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true, DraggableItems = true), PolymorphicDrawerSettings(ShowBaseType = false)]
        List<CardEffect> _inlineEffects = new List<CardEffect>();

        public string CardId => _cardId;
        public string CardName => _cardName;
        public string Description => _description;
        public int EnergyCost => _energyCost;
        public CardTag Tags => NormalizeTags(_tags);
        [BoxGroup("基础信息说明"), ShowInInspector, ReadOnly, LabelText("标签说明")]
        public string TagDescription => GetTagDescription();
        public CardActivationPosition ActivationPositions => NormalizeActivationPositions(_activationPositions);
        [BoxGroup("基础信息说明"), ShowInInspector, ReadOnly, LabelText("生效位置说明")]
        public string ActivationPositionDescription => GetActivationPositionDescription();
        public Sprite Artwork => _artwork;
        public IReadOnlyList<CardEffect> Effects => _inlineEffects;

        [BoxGroup("基础信息说明"), ShowInInspector, ReadOnly, MultiLineProperty(5), LabelText("完整描述")]
        string InspectorFullDescription => GetFullDescription();

        public bool HasTag(CardTag tag)
        {
            CardTag normalizedTag = NormalizeTags(tag);
            if (normalizedTag == CardTag.None) return Tags == CardTag.None;
            return (Tags & normalizedTag) == normalizedTag;
        }

        public bool HasAnyTag(CardTag tags)
        {
            CardTag normalizedTags = NormalizeTags(tags);
            if (normalizedTags == CardTag.None) return Tags == CardTag.None;
            return (Tags & normalizedTags) != 0;
        }

        public bool HasAllTags(CardTag tags)
        {
            CardTag normalizedTags = NormalizeTags(tags);
            if (normalizedTags == CardTag.None) return true;
            return (Tags & normalizedTags) == normalizedTags;
        }

        public string GetTagDescription()
        {
            CardTag tags = Tags;
            if (tags == CardTag.None) return "无";

            var builder = new StringBuilder();
            AppendTagName(builder, tags, CardTag.Ritual, "仪式");
            AppendTagName(builder, tags, CardTag.Spell, "法术");
            AppendTagName(builder, tags, CardTag.BattleWill, "战意");
            return builder.ToString();
        }

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
                return "任意";
            if (positions == CardActivationPosition.OddPositions)
                return "奇数";
            if (positions == CardActivationPosition.EvenPositions)
                return "偶数";

            var builder = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                CardActivationPosition position = (CardActivationPosition)(1 << i);
                if ((positions & position) == 0) continue;

                if (builder.Length > 0)
                    builder.Append(",");
                builder.Append(i + 1);
            }

            return builder.Length > 0 ? $"{builder}" : "任意位置";
        }

        public string GetFullDescription()
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_description))
                builder.AppendLine(_description.Trim());

            // foreach (CardEffect effect in _inlineEffects)
            // {
            //     if (effect == null) continue;
            //
            //     string effectDescription = effect.GetDescription();
            //     if (!string.IsNullOrWhiteSpace(effectDescription))
            //         builder.AppendLine(effectDescription);
            // }

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
            _tags = NormalizeTags(_tags);
        }

        static CardActivationPosition NormalizeActivationPositions(CardActivationPosition positions)
        {
            CardActivationPosition normalized = positions & CardActivationPosition.Any;
            return normalized == CardActivationPosition.None ? CardActivationPosition.Any : normalized;
        }

        static CardTag NormalizeTags(CardTag tags)
        {
            return tags & (CardTag.Ritual | CardTag.Spell | CardTag.BattleWill);
        }

        static void AppendTagName(StringBuilder builder, CardTag tags, CardTag tag, string tagName)
        {
            if ((tags & tag) == 0) return;

            if (builder.Length > 0)
                builder.Append("、");
            builder.Append(tagName);
        }
    }
}
