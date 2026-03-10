using Card5.Gameplay.Events;
using UnityEngine;

namespace Card5
{
    /// <summary>
    /// 战斗系统：战斗流程控制、出牌槽效果结算、回合管理。
    /// </summary>
    public class BattleSystem : AbstractSystem
    {
        static readonly int DrawPerTurn = 5;

        EnemyController _enemyController;
        BattleModel _battleModel;
        DeckModel _deckModel;
        CardSystem _cardSystem;

        protected override void OnInit()
        {
            _battleModel = this.GetModel<BattleModel>();
            _deckModel = this.GetModel<DeckModel>();
            _cardSystem = this.GetSystem<CardSystem>();
        }

        /// <summary>注册敌人控制器引用（由 EnemyController 在初始化时调用）</summary>
        public void RegisterEnemy(EnemyController enemy)
        {
            _enemyController = enemy;
        }

        /// <summary>启动战斗，初始化数据并发牌</summary>
        public void StartBattle(DeckPresetData deckPreset, EnemyData enemyData, int playerMaxHp = 30, int maxEnergy = 3)
        {
            var battleModel = this.GetModel<BattleModel>();
            var deckModel = this.GetModel<DeckModel>();
            var cardSystem = this.GetSystem<CardSystem>();

            var deckCards = deckPreset.BuildCardList();
            cardSystem.Shuffle(deckCards);

            deckModel.InitDeck(deckCards);
            battleModel.InitBattle(playerMaxHp, maxEnergy);

            if (_enemyController != null)
                _enemyController.InitEnemy(enemyData);

            this.SendEvent(new BattleStartedEvent
            {
                PlayerMaxHp = playerMaxHp,
                EnemyMaxHp = enemyData.MaxHp
            });

            StartTurn();
        }

        /// <summary>尝试将手牌中的卡放入指定槽位。handIndex 指定手牌索引以区分相同 CardData 的多张牌，若 &lt; 0 则按 CardData 查找第一张。</summary>
        public bool TryPlayCard(CardData card, int slotIndex, int handIndex = -1)
        {
            if (_battleModel.IsBattleOver) return false;
            if (slotIndex < 0 || slotIndex >= BattleModel.SlotCount) return false;
            if (!_battleModel.IsSlotEmpty(slotIndex)) return false;
            if (_battleModel.CurrentEnergy.Value < card.EnergyCost) return false;

            if (handIndex >= 0)
            {
                if (handIndex >= _deckModel.Hand.Count) return false;
                if (_deckModel.Hand[handIndex] != card) return false;
            }
            else
            {
                if (!_deckModel.Hand.Contains(card)) return false;
                handIndex = _deckModel.Hand.IndexOf(card);
            }

            _deckModel.Hand.RemoveAt(handIndex);

            _battleModel.PlaySlots[slotIndex] = card;
            _battleModel.CurrentEnergy.Value -= card.EnergyCost;

            this.SendEvent(new CardRemovedFromHandEvent { HandIndex = handIndex });
            this.SendEvent(new CardPlayedEvent { CardId = card.CardId, SlotIndex = slotIndex });
            this.SendEvent(new EnergyChangedEvent
            {
                CurrentEnergy = _battleModel.CurrentEnergy.Value,
                MaxEnergy = _battleModel.MaxEnergy.Value
            });

            return true;
        }

        /// <summary>从出牌槽撤回卡牌到手牌</summary>
        public bool TryReturnCardFromSlot(int slotIndex)
        {
            if (_battleModel.IsBattleOver) return false;
            CardData card = _battleModel.PlaySlots[slotIndex];
            if (card == null) return false;

            _battleModel.PlaySlots[slotIndex] = null;
            _battleModel.CurrentEnergy.Value += card.EnergyCost;
            _deckModel.Hand.Add(card);

            this.SendEvent(new CardAddedToHandEvent { HandIndex = _deckModel.Hand.Count - 1 });
            this.SendEvent(new CardRemovedFromSlotEvent { SlotIndex = slotIndex });
            this.SendEvent(new EnergyChangedEvent
            {
                CurrentEnergy = _battleModel.CurrentEnergy.Value,
                MaxEnergy = _battleModel.MaxEnergy.Value
            });

            return true;
        }

        /// <summary>槽位间移动或交换卡牌：fromSlot 必须有牌；toSlot 为空则移动，有牌则交换。不消耗/退还能量。</summary>
        public bool TrySwapSlots(int fromSlot, int toSlot)
        {
            if (_battleModel.IsBattleOver) return false;
            if (fromSlot == toSlot) return false;
            if (fromSlot < 0 || fromSlot >= BattleModel.SlotCount || toSlot < 0 || toSlot >= BattleModel.SlotCount)
                return false;

            CardData cardA = _battleModel.PlaySlots[fromSlot];
            if (cardA == null) return false;

            CardData cardB = _battleModel.PlaySlots[toSlot];

            _battleModel.PlaySlots[fromSlot] = cardB;
            _battleModel.PlaySlots[toSlot] = cardA;

            this.SendEvent(new SlotsSwappedEvent { SlotA = fromSlot, SlotB = toSlot });
            return true;
        }

        /// <summary>结束当前回合，结算所有槽位效果，触发敌人行动，开始新回合</summary>
        public void EndTurn()
        {
            if (_battleModel.IsBattleOver) return;

            this.SendEvent(new TurnEndedEvent { TurnNumber = _battleModel.TurnNumber.Value });

            ResolveSlots();

            if (_battleModel.IsBattleOver) return;

            EnemyTurn();

            if (_battleModel.IsBattleOver) return;

            StartTurn();
        }

        void ResolveSlots()
        {
            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                CardData card = _battleModel.PlaySlots[i];
                if (card == null) continue;

                var context = new BattleContext(
                    _battleModel,
                    _deckModel,
                    _enemyController,
                    this,
                    i,
                    card,
                    _battleModel.GetLeftNeighbor(i),
                    _battleModel.GetRightNeighbor(i)
                );

                foreach (var effect in card.Effects)
                {
                    effect.Execute(context);
                    if (_battleModel.IsBattleOver) break;
                }

                _deckModel.DiscardPile.Add(card);

                if (_battleModel.IsBattleOver) break;
            }

            _battleModel.ClearSlots();
            this.SendEvent<SlotEffectsResolvedEvent>();
        }

        void EnemyTurn()
        {
            // 敌人目前为木桩，无行动
            // 后续在此处调用 _enemyController 的行为接口
        }

        void StartTurn()
        {
            _battleModel.TurnNumber.Value++;
            _battleModel.CurrentEnergy.Value = _battleModel.MaxEnergy.Value;

            _cardSystem.DrawCards(DrawPerTurn);

            this.SendEvent(new TurnStartedEvent
            {
                TurnNumber = _battleModel.TurnNumber.Value,
                EnergyRestored = _battleModel.MaxEnergy.Value
            });

            this.SendEvent(new EnergyChangedEvent
            {
                CurrentEnergy = _battleModel.CurrentEnergy.Value,
                MaxEnergy = _battleModel.MaxEnergy.Value
            });
        }

        public void NotifyPlayerDamaged(int amount)
        {
            this.SendEvent(new DamageDealtEvent { Amount = amount, ToPlayer = true });
            NotifyPlayerHpChanged();
        }

        public void NotifyPlayerHealed(int amount)
        {
            this.SendEvent(new HealAppliedEvent { Amount = amount, ToPlayer = true });
            NotifyPlayerHpChanged();
        }

        public void NotifyPlayerHpChanged()
        {
            this.SendEvent(new PlayerHpChangedEvent
            {
                CurrentHp = _battleModel.PlayerHp.Value,
                MaxHp = _battleModel.PlayerMaxHp
            });

            if (_battleModel.PlayerHp.Value <= 0 && !_battleModel.IsBattleOver)
            {
                _battleModel.IsBattleOver = true;
                this.SendEvent<PlayerDiedEvent>();
                this.SendEvent(new BattleEndedEvent { PlayerWon = false });
            }
        }

        public void NotifyEnemyDamaged(int amount, int currentHp, int maxHp)
        {
            this.SendEvent(new DamageDealtEvent { Amount = amount, ToPlayer = false });
            NotifyEnemyHpChanged(currentHp, maxHp);
        }

        public void NotifyEnemyHealed(int amount, int currentHp, int maxHp)
        {
            this.SendEvent(new HealAppliedEvent { Amount = amount, ToPlayer = false });
            NotifyEnemyHpChanged(currentHp, maxHp);
        }

        public void NotifyEnemyHpChanged(int currentHp, int maxHp)
        {
            this.SendEvent(new EnemyHpChangedEvent { CurrentHp = currentHp, MaxHp = maxHp });

            if (currentHp <= 0 && !_battleModel.IsBattleOver)
            {
                _battleModel.IsBattleOver = true;
                this.SendEvent<EnemyDiedEvent>();
                this.SendEvent(new BattleEndedEvent { PlayerWon = true });
            }
        }
    }
}
