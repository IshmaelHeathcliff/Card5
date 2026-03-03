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

    public struct CardPlayedEvent
    {
        public string CardId;
        public int SlotIndex;
    }

    public struct CardRemovedFromSlotEvent
    {
        public int SlotIndex;
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
}
