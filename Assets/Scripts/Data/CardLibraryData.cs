using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "NewCardLibrary", menuName = "Card5/Card Library")]
    public class CardLibraryData : ScriptableObject
    {
        [SerializeField, ListDrawerSettings(ShowPaging = true, DraggableItems = true)] List<CardLibraryEntry> _entries = new List<CardLibraryEntry>();

        public IReadOnlyList<CardLibraryEntry> Entries => _entries;
    }

    [Serializable]
    public class CardLibraryEntry
    {
        [SerializeField, Required] CardData _card;
        [SerializeField, MinValue(1)] int _weight = 1;
        [SerializeField, ListDrawerSettings(ShowPaging = false)] List<CardUnlockCondition> _unlockConditions = new List<CardUnlockCondition>();

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
                case CardUnlockConditionType.MinTurnNumber:
                    return context.TurnNumber >= _value;
                case CardUnlockConditionType.MaxTurnNumber:
                    return context.TurnNumber <= _value;
                case CardUnlockConditionType.MinDeckCardCount:
                    return context.DeckCardCount >= _value;
                case CardUnlockConditionType.HasCardInDeck:
                    return _card != null && context.HasCardInDeck(_card);
                case CardUnlockConditionType.DoesNotHaveCardInDeck:
                    return _card != null && !context.HasCardInDeck(_card);
                case CardUnlockConditionType.PlayerHpPercentAtMost:
                    return context.PlayerHpPercent <= _value;
                default:
                    return false;
            }
        }

        bool UsesIntValue => _conditionType == CardUnlockConditionType.MinTurnNumber
            || _conditionType == CardUnlockConditionType.MaxTurnNumber
            || _conditionType == CardUnlockConditionType.MinDeckCardCount
            || _conditionType == CardUnlockConditionType.PlayerHpPercentAtMost;

        bool UsesCardValue => _conditionType == CardUnlockConditionType.HasCardInDeck
            || _conditionType == CardUnlockConditionType.DoesNotHaveCardInDeck;
    }

    public enum CardUnlockConditionType
    {
        Always,
        MinTurnNumber,
        MaxTurnNumber,
        MinDeckCardCount,
        HasCardInDeck,
        DoesNotHaveCardInDeck,
        PlayerHpPercentAtMost
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

        public int TurnNumber => _battleModel != null ? _battleModel.TurnNumber.Value : 0;
        public int DeckCardCount => _deckModel != null ? _deckModel.FullDeck.Count : 0;

        public int PlayerHpPercent
        {
            get
            {
                if (_battleModel == null || _battleModel.PlayerMaxHp <= 0)
                    return 0;
                return Mathf.RoundToInt((float)_battleModel.PlayerHp.Value / _battleModel.PlayerMaxHp * 100f);
            }
        }

        public bool HasCardInDeck(CardData card)
        {
            if (_deckModel == null || card == null) return false;
            return _deckModel.FullDeck.Contains(card);
        }
    }
}
