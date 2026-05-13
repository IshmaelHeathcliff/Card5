using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

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

    public enum CardType
    {
        [InspectorName("通用")]
        Common = 0,
        [InspectorName("占卜")]
        Divination = 1,
        [InspectorName("奥术")]
        Arcane = 2
    }

    [HideMonoScript]
    [CreateAssetMenu(fileName = "NewCard", menuName = "Card5/Card")]
    public class CardData : SerializedScriptableObject
    {
        [HorizontalGroup("基础信息首行", 0.68f)]
        [VerticalGroup("基础信息首行/文本列")]
        [BoxGroup("基础信息首行/文本列/基础信息文本"), SerializeField, LabelText("卡牌ID"), MinValue(0)] int _cardId;
        [HorizontalGroup("基础信息首行")]
        [VerticalGroup("基础信息首行/文本列")]
        [BoxGroup("基础信息首行/文本列/基础信息文本"), SerializeField, LabelText("卡牌名称")] string _cardName;
        [HorizontalGroup("基础信息首行")]
        [VerticalGroup("基础信息首行/文本列")]
        [BoxGroup("基础信息首行/文本列/基础信息文本"), SerializeField, LabelText("描述"), TextArea(3, 6)] string _description;
        [HorizontalGroup("基础信息首行")]
        [VerticalGroup("基础信息首行/文本列")]
        [BoxGroup("基础信息首行/文本列/基础信息文本"), SerializeField, LabelText("能量消耗"), MinValue(0)] int _energyCost;
        [BoxGroup("基础信息配置"), FormerlySerializedAs("_tags"), SerializeField, LabelText("卡牌类型"), EnumToggleButtons] CardType _cardType = CardType.Common;
        [HorizontalGroup("基础信息首行", 0.32f)]
        [VerticalGroup("基础信息首行/视觉列")]
        [BoxGroup("基础信息首行/视觉列/基础信息视觉"), SerializeField, LabelText("卡面图片"), PreviewField(180, ObjectFieldAlignment.Center)] Sprite _artwork;
        [BoxGroup("效果配置")]
        [OdinSerialize, LabelText("卡牌效果"), ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true, DraggableItems = true), PolymorphicDrawerSettings(ShowBaseType = false)]
        List<CardEffect> _inlineEffects = new List<CardEffect>();

        public int CardId => _cardId;
        public string CardName => _cardName;
        public string Description => _description;
        public int EnergyCost => _energyCost;
        public CardType Type => NormalizeCardType(_cardType);
        [BoxGroup("基础信息说明"), ShowInInspector, ReadOnly, LabelText("类型说明")]
        public string TypeDescription => GetTypeDescription();
        public Sprite Artwork => _artwork;
        public IReadOnlyList<CardEffect> Effects => _inlineEffects;

        [BoxGroup("基础信息说明"), ShowInInspector, ReadOnly, MultiLineProperty(5), LabelText("完整描述")]
        string InspectorFullDescription => GetFullDescription();

        public bool IsType(CardType type)
        {
            return Type == NormalizeCardType(type);
        }

        public string GetTypeDescription()
        {
            return Type switch
            {
                CardType.Common     => "通用",
                CardType.Divination => "占卜",
                CardType.Arcane     => "奥术",
                _                   => "通用"
            };
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

        void OnValidate()
        {
            _cardType = NormalizeCardType(_cardType);
        }

        static CardType NormalizeCardType(CardType cardType)
        {
            return cardType is CardType.Common or CardType.Divination or CardType.Arcane ? cardType : CardType.Common;
        }
    }
}
