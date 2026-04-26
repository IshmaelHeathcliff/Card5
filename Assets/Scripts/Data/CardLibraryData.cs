using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "NewCardLibrary", menuName = "Card5/Card Library")]
    public class CardLibraryData : ScriptableObject
    {
        [BoxGroup("概览"), ShowInInspector, ReadOnly, LabelText("条目数量")]
        int EntryCount => _entries.Count;

        [BoxGroup("概览"), ShowInInspector, ReadOnly, LabelText("总权重")]
        int TotalWeight => _entries.Sum(entry => entry != null ? entry.Weight : 0);

        [BoxGroup("牌库条目")]
        [SerializeField, LabelText("牌库条目"), ListDrawerSettings(ShowPaging = true, DraggableItems = true, DefaultExpandedState = true)]
        [Searchable]
        List<CardLibraryEntry> _entries = new List<CardLibraryEntry>();

        public IReadOnlyList<CardLibraryEntry> Entries => _entries;
    }

    [Serializable]
    public class CardLibraryEntry
    {
        [BoxGroup("基础信息"), SerializeField, LabelText("卡牌"), Required] CardData _card;
        [BoxGroup("基础信息"), SerializeField, LabelText("权重"), MinValue(1)] int _weight = 1;
        [BoxGroup("基础信息"), ShowInInspector, ReadOnly, PreviewField(80, ObjectFieldAlignment.Left), LabelText("卡面预览")]
        Sprite ArtworkPreview => _card != null ? _card.Artwork : null;
        [BoxGroup("基础信息"), ShowInInspector, ReadOnly, LabelText("条目说明")]
        string EntrySummary => GetSummary();

        [BoxGroup("解锁条件")]
        [SerializeField, LabelText("解锁条件"), ListDrawerSettings(ShowPaging = false, DefaultExpandedState = true)]
        List<CardUnlockCondition> _unlockConditions = new List<CardUnlockCondition>();

        public CardData Card => _card;
        public int Weight => _weight;
        public IReadOnlyList<CardUnlockCondition> UnlockConditions => _unlockConditions;

        public bool IsUnlocked(CardUnlockContext context)
        {
            foreach (CardUnlockCondition condition in _unlockConditions)
            {
                if (condition != null && !condition.IsMet(context))
                    return false;
            }

            return true;
        }

        string GetSummary()
        {
            string cardName = _card != null ? _card.CardName : "未配置卡牌";
            return $"[{cardName}] 权重 {_weight}，条件 {_unlockConditions.Count} 条";
        }

        public override string ToString()
        {
            return GetSummary();
        }
    }

    [Serializable]
    public class CardUnlockCondition
    {
        [BoxGroup("条件"), SerializeField, LabelText("条件类型")] CardUnlockConditionType _conditionType = CardUnlockConditionType.Always;
        [BoxGroup("条件"), SerializeField, LabelText("数值"), ShowIf(nameof(UsesIntValue)), MinValue(0)] int _value;
        [BoxGroup("条件"), SerializeField, LabelText("卡牌"), ShowIf(nameof(UsesCardValue))] CardData _card;

        public CardUnlockConditionType ConditionType => _conditionType;
        public int Value => _value;
        public CardData Card => _card;

        [BoxGroup("条件"), ShowInInspector, ReadOnly, LabelText("条件说明")]
        string ConditionSummary => GetSummary();

        public bool IsMet(CardUnlockContext context)
        {
            if (context == null) return false;

            switch (_conditionType)
            {
                case CardUnlockConditionType.Always:
                    return true;
                case CardUnlockConditionType.MinBattleCount:
                    return context.BattleCount >= _value;
                case CardUnlockConditionType.MaxBattleCount:
                    return context.BattleCount <= _value;
                case CardUnlockConditionType.MinDeckCardCount:
                    return context.DeckCardCount >= _value;
                case CardUnlockConditionType.HasCardInDeck:
                    return _card != null && context.HasCardInDeck(_card);
                case CardUnlockConditionType.DoesNotHaveCardInDeck:
                    return _card != null && !context.HasCardInDeck(_card);
                default:
                    return false;
            }
        }

        bool UsesIntValue => _conditionType == CardUnlockConditionType.MinBattleCount
            || _conditionType == CardUnlockConditionType.MaxBattleCount
            || _conditionType == CardUnlockConditionType.MinDeckCardCount;

        bool UsesCardValue => _conditionType == CardUnlockConditionType.HasCardInDeck
            || _conditionType == CardUnlockConditionType.DoesNotHaveCardInDeck;

        string GetSummary()
        {
            return _conditionType switch
            {
                CardUnlockConditionType.Always                => "始终解锁",
                CardUnlockConditionType.MinBattleCount        => $"战斗次数至少 {_value}",
                CardUnlockConditionType.MaxBattleCount        => $"战斗次数至多 {_value}",
                CardUnlockConditionType.MinDeckCardCount      => $"牌组数量至少 {_value}",
                CardUnlockConditionType.HasCardInDeck         => $"牌组中拥有《{_card?.CardName ?? "未配置"}》",
                CardUnlockConditionType.DoesNotHaveCardInDeck => $"牌组中没有《{_card?.CardName ?? "未配置"}》",
                _                                             => "未配置条件"
            };
        }

        public override string ToString()
        {
            return GetSummary();
        }
    }

    public enum CardUnlockConditionType
    {
        [InspectorName("始终解锁")]
        Always,
        [InspectorName("最少战斗次数")]
        MinBattleCount,
        [InspectorName("最多战斗次数")]
        MaxBattleCount,
        [InspectorName("最少牌组数量")]
        MinDeckCardCount,
        [InspectorName("牌组中拥有卡牌")]
        HasCardInDeck,
        [InspectorName("牌组中没有卡牌")]
        DoesNotHaveCardInDeck
    }

    public class CardUnlockContext
    {
        readonly BattleModel _battleModel;
        readonly DeckModel _deckModel;

        public CardUnlockContext(BattleModel battleModel, DeckModel deckModel)
        {
            _battleModel = battleModel;
            _deckModel = deckModel;
        }

        public int BattleCount => _battleModel != null ? _battleModel.CurrentMonsterIndex + 1 : 0;
        public int DeckCardCount => _deckModel != null ? _deckModel.FullDeck.Count : 0;

        public bool HasCardInDeck(CardData card)
        {
            if (_deckModel == null || card == null) return false;
            return _deckModel.FullDeck.Contains(card);
        }
    }
}
