using System;
using System.Collections.Generic;
using UnityEngine;

namespace Card5
{
    /// <summary>
    /// 单张卡牌生效期间的上下文，效果通过此类访问战场状态、记录信息并注册临时事件。
    /// BattleSystem 负责在生效结束时释放该对象。
    /// </summary>
    public class BattleContext : IDisposable
    {
        readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        readonly List<BattleContextUnRegister> _unRegisters = new List<BattleContextUnRegister>();

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

        /// <summary>上下文是否已释放。释放后会清空记录并注销注册过的事件。</summary>
        public bool IsDisposed { get; private set; }

        bool _useCardEffectBoost;

        internal bool UseCardEffectBoost => _useCardEffectBoost;

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
            ThrowIfDisposed();
            int baseAmount = amount;
            amount = GetModifiedDamage(amount);
            if (Enemy == null || amount <= 0)
            {
                Debug.Log($"[CardEffect] 伤害跳过 | 卡牌={FormatCard(CurrentCard)} | 槽位={FormatSlot(SlotIndex)} | 基础伤害={baseAmount} | 结算伤害={amount} | 原因={(Enemy == null ? "无敌人" : "伤害不大于0")}");
                return;
            }

            int damageBefore = DamageDealtThisCard;
            Enemy.TakeDamage(amount);
            DamageDealtThisCard += amount;
            if (BattleModel != null && BattleModel.IsCurrentMonsterDefeated)
                DefeatedEnemyThisCard = true;

            Debug.Log($"[CardEffect] 伤害结算 | 卡牌={FormatCard(CurrentCard)} | 槽位={FormatSlot(SlotIndex)} | 基础伤害={baseAmount} | 结算伤害={amount} | 本次前累计={damageBefore} | 本次后累计={DamageDealtThisCard} | 击败怪物={DefeatedEnemyThisCard}");
        }

        public void SetValue<T>(string key, T value)
        {
            ThrowIfDisposed();
            ValidateKey(key);
            _values[key] = value;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            ThrowIfDisposed();
            ValidateKey(key);

            if (_values.TryGetValue(key, out object rawValue) && rawValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default(T);
            return false;
        }

        public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
        {
            return TryGetValue(key, out T value) ? value : defaultValue;
        }

        public bool HasValue(string key)
        {
            ThrowIfDisposed();
            ValidateKey(key);
            return _values.ContainsKey(key);
        }

        public bool RemoveValue(string key)
        {
            ThrowIfDisposed();
            ValidateKey(key);
            return _values.Remove(key);
        }

        public void ClearValues()
        {
            ThrowIfDisposed();
            _values.Clear();
        }

        public IUnRegister RegisterEvent<T>(Action<T> onEvent)
        {
            ThrowIfDisposed();
            if (onEvent == null) throw new ArgumentNullException(nameof(onEvent));
            if (BattleSystem == null) throw new InvalidOperationException("当前上下文没有可用的 BattleSystem，无法注册事件。");

            return TrackUnRegister(BattleSystem.RegisterEvent(onEvent));
        }

        public IUnRegister TrackUnRegister(IUnRegister unRegister)
        {
            ThrowIfDisposed();
            if (unRegister == null) throw new ArgumentNullException(nameof(unRegister));

            var trackedUnRegister = new BattleContextUnRegister(this, unRegister);
            _unRegisters.Add(trackedUnRegister);
            return trackedUnRegister;
        }

        public void Dispose()
        {
            if (IsDisposed) return;

            while (_unRegisters.Count > 0)
            {
                _unRegisters[_unRegisters.Count - 1].UnRegister();
            }

            _values.Clear();
            IsDisposed = true;
        }

        internal void SetUseCardEffectBoost(bool useCardEffectBoost)
        {
            _useCardEffectBoost = useCardEffectBoost;
        }

        int GetModifiedDamage(int amount)
        {
            if (!_useCardEffectBoost) return amount;
            return BattleSystem != null ? BattleSystem.ModifyCardDamage(SlotIndex, amount) : amount;
        }

        void RemoveTrackedUnRegister(BattleContextUnRegister unRegister)
        {
            _unRegisters.Remove(unRegister);
        }

        void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(BattleContext));
        }

        static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("上下文键不能为空。", nameof(key));
        }

        static string FormatCard(CardData card)
        {
            if (card == null) return "空";

            string cardName = string.IsNullOrWhiteSpace(card.CardName) ? card.name : card.CardName;
            return $"{cardName}(ID:{card.CardId}, 类型:{card.Type})";
        }

        static string FormatSlot(int slotIndex)
        {
            return slotIndex >= 0 ? $"{slotIndex + 1}号位" : "无槽位";
        }

        sealed class BattleContextUnRegister : IUnRegister
        {
            readonly BattleContext _context;
            readonly IUnRegister _inner;
            bool _isUnregistered;

            public BattleContextUnRegister(BattleContext context, IUnRegister inner)
            {
                _context = context;
                _inner = inner;
            }

            public void UnRegister()
            {
                if (_isUnregistered) return;

                _isUnregistered = true;
                _inner.UnRegister();
                _context.RemoveTrackedUnRegister(this);
            }
        }
    }
}
