namespace Card5
{
    /// <summary>
    /// 卡牌效果执行的上下文，效果通过此类访问战场状态并执行操作。
    /// BattleSystem 在结算每张牌时构造该对象。
    /// </summary>
    public class BattleContext
    {
        public BattleModel BattleModel { get; }
        public DeckModel DeckModel { get; }
        public EnemyController Enemy { get; }
        public BattleSystem BattleSystem { get; }

        /// <summary>当前出牌所在的槽位索引（0-4）</summary>
        public int SlotIndex { get; }

        /// <summary>当前槽位左侧相邻的牌数据（不存在则为 null）</summary>
        public CardData LeftNeighbor { get; }

        /// <summary>当前槽位右侧相邻的牌数据（不存在则为 null）</summary>
        public CardData RightNeighbor { get; }

        /// <summary>当前正在结算的牌数据</summary>
        public CardData CurrentCard { get; }

        /// <summary>当前正在执行的卡牌效果时机</summary>
        public CardEffectTiming CurrentTiming { get; internal set; }

        /// <summary>本张卡在出牌阶段累计造成的伤害，用于出牌结束阶段效果判断</summary>
        public int DamageDealtThisCard { get; private set; }

        /// <summary>本张卡是否在出牌阶段击败了当前怪物</summary>
        public bool DefeatedEnemyThisCard { get; private set; }

        bool _useCardEffectBoost;

        public BattleContext(
            BattleModel battleModel,
            DeckModel deckModel,
            EnemyController enemy,
            BattleSystem battleSystem,
            int slotIndex,
            CardData currentCard,
            CardData leftNeighbor,
            CardData rightNeighbor)
        {
            BattleModel = battleModel;
            DeckModel = deckModel;
            Enemy = enemy;
            BattleSystem = battleSystem;
            SlotIndex = slotIndex;
            CurrentCard = currentCard;
            LeftNeighbor = leftNeighbor;
            RightNeighbor = rightNeighbor;
        }

        /// <summary>对敌人造成伤害</summary>
        public void DealDamage(int amount)
        {
            amount = GetModifiedEffectAmount(amount);
            if (Enemy == null || amount <= 0) return;

            Enemy.TakeDamage(amount);
            DamageDealtThisCard += amount;
            if (BattleModel != null && BattleModel.IsCurrentMonsterDefeated)
                DefeatedEnemyThisCard = true;
        }

        internal void SetUseCardEffectBoost(bool useCardEffectBoost)
        {
            _useCardEffectBoost = useCardEffectBoost;
        }

        int GetModifiedEffectAmount(int amount)
        {
            if (!_useCardEffectBoost) return amount;
            return BattleSystem != null ? BattleSystem.ModifyCardEffectAmount(SlotIndex, amount) : amount;
        }
    }
}
