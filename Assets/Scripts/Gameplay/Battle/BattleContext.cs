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
        public MarkSystem MarkSystem { get; }

        /// <summary>当前出牌所在的槽位索引（0-4）</summary>
        public int SlotIndex { get; }

        /// <summary>当前槽位左侧相邻的牌数据（不存在则为 null）</summary>
        public CardData LeftNeighbor { get; }

        /// <summary>当前槽位右侧相邻的牌数据（不存在则为 null）</summary>
        public CardData RightNeighbor { get; }

        /// <summary>当前正在结算的牌数据</summary>
        public CardData CurrentCard { get; }

        public BattleContext(
            BattleModel battleModel,
            DeckModel deckModel,
            EnemyController enemy,
            BattleSystem battleSystem,
            MarkSystem markSystem,
            int slotIndex,
            CardData currentCard,
            CardData leftNeighbor,
            CardData rightNeighbor)
        {
            BattleModel = battleModel;
            DeckModel = deckModel;
            Enemy = enemy;
            BattleSystem = battleSystem;
            MarkSystem = markSystem;
            SlotIndex = slotIndex;
            CurrentCard = currentCard;
            LeftNeighbor = leftNeighbor;
            RightNeighbor = rightNeighbor;
        }

        /// <summary>对指定目标造成伤害</summary>
        public void DealDamage(int amount, DamageTarget target)
        {
            if (target == DamageTarget.Enemy)
            {
                Enemy.TakeDamage(amount);
            }
            else
            {
                int newHp = BattleModel.PlayerHp.Value - amount;
                BattleModel.PlayerHp.Value = newHp < 0 ? 0 : newHp;
                BattleSystem.NotifyPlayerDamaged(amount);
            }
        }

        /// <summary>对指定目标恢复生命值</summary>
        public void ApplyHeal(int amount, HealTarget target)
        {
            if (target == HealTarget.Player)
            {
                int newHp = BattleModel.PlayerHp.Value + amount;
                int maxHp = BattleModel.PlayerMaxHp;
                BattleModel.PlayerHp.Value = newHp > maxHp ? maxHp : newHp;
                BattleSystem.NotifyPlayerHealed(amount);
            }
            else
            {
                Enemy.Heal(amount);
            }
        }
    }
}
