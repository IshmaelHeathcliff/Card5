using System.Collections.Generic;

namespace Card5.Gameplay.Events
{
    public struct BattleStartedEvent
    {
        public int PlayerMaxHp;
        public int EnemyMaxHp;
    }

    public struct BattleEndedEvent
    {
        public bool PlayerWon;
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
        public string CardId;
        public int HandIndex;
    }

    public struct HandRefreshedEvent
    {
        public List<string> CardIds;
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

    public struct CardPlayedEvent
    {
        public string CardId;
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

    public struct SlotEffectsResolvedEvent
    {
    }

    public struct DamageDealtEvent
    {
        public int Amount;
        public bool ToPlayer;
    }

    public struct HealAppliedEvent
    {
        public int Amount;
        public bool ToPlayer;
    }

    public struct PlayerHpChangedEvent
    {
        public int CurrentHp;
        public int MaxHp;
    }

    public struct EnemyHpChangedEvent
    {
        public int CurrentHp;
        public int MaxHp;
    }

    public struct EnemyDiedEvent
    {
    }

    public struct PlayerDiedEvent
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

    /// <summary>印记被施加时发送</summary>
    public struct MarkAppliedEvent
    {
        public string MarkId;
        public MarkTargetType TargetType;
        /// <summary>槽位印记时有效</summary>
        public int SlotIndex;
        /// <summary>卡牌印记时有效</summary>
        public string CardId;
    }

    /// <summary>印记到期或被移除时发送</summary>
    public struct MarkRemovedEvent
    {
        public string MarkId;
        public MarkTargetType TargetType;
        public int SlotIndex;
        public string CardId;
    }
}
