namespace Card5
{
    /// <summary>
    /// 战斗状态 Model：玩家 HP、能量、出牌槽、回合计数。
    /// 不存储手牌/牌堆，这些由 DeckModel 管理。
    /// </summary>
    public class BattleModel : AbstractModel
    {
        public const int SlotCount = 5;

        public BindableProperty<int> PlayerHp { get; } = new BindableProperty<int>();
        public int PlayerMaxHp { get; private set; }

        public BindableProperty<int> CurrentEnergy { get; } = new BindableProperty<int>();
        public BindableProperty<int> MaxEnergy { get; } = new BindableProperty<int>(3);

        public BindableProperty<int> TurnNumber { get; } = new BindableProperty<int>(0);

        /// <summary>5 个出牌槽，null 表示空槽</summary>
        public CardData[] PlaySlots { get; } = new CardData[SlotCount];

        public bool IsBattleOver { get; set; }

        /// <summary>每回合可重抽次数</summary>
        public int RedrawsPerTurn { get; set; } = 1;

        /// <summary>当前回合剩余重抽次数</summary>
        public int RedrawsRemaining { get; set; }

        protected override void OnInit()
        {
        }

        public void InitBattle(int playerMaxHp, int maxEnergy)
        {
            PlayerMaxHp = playerMaxHp;
            PlayerHp.Value = playerMaxHp;
            MaxEnergy.Value = maxEnergy;
            CurrentEnergy.Value = maxEnergy;
            TurnNumber.Value = 0;
            IsBattleOver = false;
            RedrawsRemaining = RedrawsPerTurn;
            ClearSlots();
        }

        public void ClearSlots()
        {
            for (int i = 0; i < SlotCount; i++)
                PlaySlots[i] = null;
        }

        public bool IsSlotEmpty(int slotIndex) => PlaySlots[slotIndex] == null;

        public CardData GetLeftNeighbor(int slotIndex)
        {
            int left = slotIndex - 1;
            return left >= 0 ? PlaySlots[left] : null;
        }

        public CardData GetRightNeighbor(int slotIndex)
        {
            int right = slotIndex + 1;
            return right < SlotCount ? PlaySlots[right] : null;
        }
    }
}
