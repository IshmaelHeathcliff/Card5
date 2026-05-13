using System.Collections.Generic;

namespace Card5.Gameplay.Events
{
    public struct BattleStartedEvent
    {
        public int EnemyMaxHp;
    }

    public struct BattleEndedEvent
    {
        public bool PlayerWon;
    }

    public struct MonsterStartedEvent
    {
        public int MonsterIndex;
        public int MonsterCount;
        public string EnemyName;
        public int EnemyMaxHp;
        public int MaxPlayRounds;
    }

    public struct MonsterPlayRoundCountChangedEvent
    {
        public int CurrentRound;
        public int MaxCount;
    }

    public struct TurnStartedEvent
    {
        public int TurnNumber;
        public int EnergyRestored;
    }

    public struct TurnEndedEvent
    {
        public int TurnNumber;
    }

    public struct CardDrawnEvent
    {
        public int CardId;
        public int HandIndex;
    }

    public struct HandRefreshedEvent
    {
        public List<int> CardIds;
    }

    public struct HandDiscardedEvent
    {
    }

    /// <summary>单张手牌被移出（打出或弃掉）时发送，HandIndex 为移除前在手中的索引</summary>
    public struct CardRemovedFromHandEvent
    {
        public int HandIndex;
    }

    /// <summary>单张手牌被加入手中时发送，HandIndex 为加入后在手中的索引</summary>
    public struct CardAddedToHandEvent
    {
        public int HandIndex;
    }

    public struct CardReturnedToHandEvent
    {
        public int CardId;
        public int HandIndex;
        public int SourceSlotIndex;
    }

    public struct CardPlayedEvent
    {
        public int CardId;
        public int SlotIndex;
    }

    public struct CardRemovedFromSlotEvent
    {
        public int SlotIndex;
    }

    /// <summary>两个槽位交换或移动卡牌后发送，SlotA、SlotB 为涉及的两个槽位索引</summary>
    public struct SlotsSwappedEvent
    {
        public int SlotA;
        public int SlotB;
    }

    public struct HandSlotSwappedEvent
    {
        public int HandCardId;
        public int SlotCardId;
        public int HandIndex;
        public int SlotIndex;
    }

    public struct SlotEffectsResolvedEvent
    {
    }

    public struct DamageDealtEvent
    {
        public int Amount;
    }

    public struct EnemyHpChangedEvent
    {
        public int CurrentHp;
        public int MaxHp;
    }

    public struct EnemyDiedEvent
    {
    }

    public struct EnergyChangedEvent
    {
        public int CurrentEnergy;
        public int MaxEnergy;
    }

    public struct RedrawCountChangedEvent
    {
        public int Remaining;
        public int Max;
    }

    /// <summary>抽牌堆数量变化时发送</summary>
    public struct DrawPileChangedEvent
    {
        public int Count;
    }

    /// <summary>弃牌堆数量变化时发送</summary>
    public struct DiscardPileChangedEvent
    {
        public int Count;
    }

    public struct DiscardPileShuffledIntoDrawEvent
    {
        public IReadOnlyList<CardData> Cards;
        public int Count;
    }

    public struct DrawPileDiscardedEvent
    {
        public IReadOnlyList<CardData> Cards;
        public int Count;
    }

    /// <summary>新卡牌加入牌库（写入弃牌堆并同步 FullDeck）时发送</summary>
    public struct CardAddedToDeckEvent
    {
        public int CardId;
        public int DrawPileCount;
        public int DiscardPileCount;
    }

    /// <summary>卡牌从牌库移除（FullDeck、DrawPile 或 DiscardPile）时发送</summary>
    public struct CardRemovedFromDeckEvent
    {
        public int CardId;
        public int DrawPileCount;
        public int DiscardPileCount;
    }

    public struct BattleRewardOfferedEvent
    {
        public int RewardId;
        public IReadOnlyList<BattleRewardOffer> Offers;
    }

    public struct BattleRewardOptionClaimedEvent
    {
        public int RewardId;
        public string OfferId;
        public string OptionId;
        public BattleRewardType RewardType;
        public int CardId;
        public IReadOnlyList<BattleRewardOffer> RemainingOffers;
    }

    public struct BattleRewardCompletedEvent
    {
        public int RewardId;
    }
}
