using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "NewCardLibrary", menuName = "Card5/Card Library")]
    public class CardLibraryData : ScriptableObject
    {
        [SerializeField, LabelText("牌库条目"), ListDrawerSettings(ShowPaging = true, DraggableItems = true)] List<CardLibraryEntry> _entries = new List<CardLibraryEntry>();

        public IReadOnlyList<CardLibraryEntry> Entries => _entries;
    }

    [Serializable]
    public class CardLibraryEntry
    {
        [SerializeField, LabelText("卡牌"), Required] CardData _card;
        [SerializeField, LabelText("权重"), MinValue(1)] int _weight = 1;
        [SerializeField, LabelText("解锁条件"), ListDrawerSettings(ShowPaging = false)] List<CardUnlockCondition> _unlockConditions = new List<CardUnlockCondition>();

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
    }

    [Serializable]
    public class CardUnlockCondition
    {
        [SerializeField, LabelText("条件类型")] CardUnlockConditionType _conditionType = CardUnlockConditionType.Always;
        [SerializeField, LabelText("数值"), ShowIf(nameof(UsesIntValue))] int _value;
        [SerializeField, LabelText("卡牌"), ShowIf(nameof(UsesCardValue))] CardData _card;

        public CardUnlockConditionType ConditionType => _conditionType;
        public int Value => _value;
        public CardData Card => _card;

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
