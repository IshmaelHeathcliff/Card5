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

        protected override void OnInit()
        {
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

        /// <summary>尝试将手牌中的卡放入指定槽位</summary>
        public bool TryPlayCard(CardData card, int slotIndex)
        {
            var battleModel = this.GetModel<BattleModel>();
            var deckModel = this.GetModel<DeckModel>();

            if (battleModel.IsBattleOver) return false;
            if (slotIndex < 0 || slotIndex >= BattleModel.SlotCount) return false;
            if (!battleModel.IsSlotEmpty(slotIndex)) return false;
            if (!deckModel.Hand.Contains(card)) return false;
            if (battleModel.CurrentEnergy.Value < card.EnergyCost) return false;

            deckModel.Hand.Remove(card);
            battleModel.PlaySlots[slotIndex] = card;
            battleModel.CurrentEnergy.Value -= card.EnergyCost;

            this.SendEvent(new CardPlayedEvent { CardId = card.CardId, SlotIndex = slotIndex });
            this.SendEvent(new EnergyChangedEvent
            {
                CurrentEnergy = battleModel.CurrentEnergy.Value,
                MaxEnergy = battleModel.MaxEnergy.Value
            });

            var cardIds = new System.Collections.Generic.List<string>();
            foreach (var c in deckModel.Hand)
                cardIds.Add(c.CardId);
            this.SendEvent(new HandRefreshedEvent { CardIds = cardIds });

            return true;
        }

        /// <summary>从出牌槽撤回卡牌到手牌</summary>
        public bool TryReturnCardFromSlot(int slotIndex)
        {
            var battleModel = this.GetModel<BattleModel>();
            var deckModel = this.GetModel<DeckModel>();

            if (battleModel.IsBattleOver) return false;
            var card = battleModel.PlaySlots[slotIndex];
            if (card == null) return false;

            battleModel.PlaySlots[slotIndex] = null;
            battleModel.CurrentEnergy.Value += card.EnergyCost;
            deckModel.Hand.Add(card);

            this.SendEvent(new CardRemovedFromSlotEvent { SlotIndex = slotIndex });
            this.SendEvent(new EnergyChangedEvent
            {
                CurrentEnergy = battleModel.CurrentEnergy.Value,
                MaxEnergy = battleModel.MaxEnergy.Value
            });

            var cardIds = new System.Collections.Generic.List<string>();
            foreach (var c in deckModel.Hand)
                cardIds.Add(c.CardId);
            this.SendEvent(new HandRefreshedEvent { CardIds = cardIds });

            return true;
        }

        /// <summary>结束当前回合，结算所有槽位效果，触发敌人行动，开始新回合</summary>
        public void EndTurn()
        {
            var battleModel = this.GetModel<BattleModel>();
            if (battleModel.IsBattleOver) return;

            this.SendEvent(new TurnEndedEvent { TurnNumber = battleModel.TurnNumber.Value });

            ResolveSlots();

            if (battleModel.IsBattleOver) return;

            EnemyTurn();

            if (battleModel.IsBattleOver) return;

            StartTurn();
        }

        void ResolveSlots()
        {
            var battleModel = this.GetModel<BattleModel>();
            var deckModel = this.GetModel<DeckModel>();

            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                var card = battleModel.PlaySlots[i];
                if (card == null) continue;

                var context = new BattleContext(
                    battleModel,
                    deckModel,
                    _enemyController,
                    this,
                    i,
                    card,
                    battleModel.GetLeftNeighbor(i),
                    battleModel.GetRightNeighbor(i)
                );

                foreach (var effect in card.Effects)
                {
                    effect.Execute(context);
                    if (battleModel.IsBattleOver) break;
                }

                deckModel.DiscardPile.Add(card);

                if (battleModel.IsBattleOver) break;
            }

            battleModel.ClearSlots();
            this.SendEvent<SlotEffectsResolvedEvent>();
        }

        void EnemyTurn()
        {
            // 敌人目前为木桩，无行动
            // 后续在此处调用 _enemyController 的行为接口
        }

        void StartTurn()
        {
            var battleModel = this.GetModel<BattleModel>();
            var cardSystem = this.GetSystem<CardSystem>();

            battleModel.TurnNumber.Value++;
            battleModel.CurrentEnergy.Value = battleModel.MaxEnergy.Value;

            cardSystem.DrawCards(DrawPerTurn);

            this.SendEvent(new TurnStartedEvent
            {
                TurnNumber = battleModel.TurnNumber.Value,
                EnergyRestored = battleModel.MaxEnergy.Value
            });

            this.SendEvent(new EnergyChangedEvent
            {
                CurrentEnergy = battleModel.CurrentEnergy.Value,
                MaxEnergy = battleModel.MaxEnergy.Value
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
            var battleModel = this.GetModel<BattleModel>();
            this.SendEvent(new PlayerHpChangedEvent
            {
                CurrentHp = battleModel.PlayerHp.Value,
                MaxHp = battleModel.PlayerMaxHp
            });

            if (battleModel.PlayerHp.Value <= 0 && !battleModel.IsBattleOver)
            {
                battleModel.IsBattleOver = true;
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

            var battleModel = this.GetModel<BattleModel>();
            if (currentHp <= 0 && !battleModel.IsBattleOver)
            {
                battleModel.IsBattleOver = true;
                this.SendEvent<EnemyDiedEvent>();
                this.SendEvent(new BattleEndedEvent { PlayerWon = true });
            }
        }
    }
}
