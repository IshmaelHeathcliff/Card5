using System.Collections.Generic;
using Card5.Gameplay.Events;
using UnityEngine;

namespace Card5
{
    /// <summary>
    /// 战斗系统：战斗流程控制、出牌槽效果结算、回合管理。
    /// </summary>
    public class BattleSystem : AbstractSystem
    {
        static readonly int DrawPerTurn = 8;

        EnemyController _enemyController;
        BattleModel _battleModel;
        DeckModel _deckModel;
        CardSystem _cardSystem;
        MarkSystem _markSystem;
        BattleRewardSystem _rewardSystem;
        BattleRewardModel _rewardModel;
        DeckPresetData _lastDeckPreset;
        MonsterListData _lastMonsterList;
        EnemyData _lastEnemyData;
        BattleRewardConfigData _lastRewardConfig;
        int _lastPlayerMaxHp;
        int _lastMaxEnergy;
        readonly List<SlotCardEffectBoost> _slotCardEffectBoosts = new List<SlotCardEffectBoost>();

        protected override void OnInit()
        {
            _battleModel = this.GetModel<BattleModel>();
            _deckModel = this.GetModel<DeckModel>();
            _cardSystem = this.GetSystem<CardSystem>();
            _markSystem = this.GetSystem<MarkSystem>();
            _rewardSystem = this.GetSystem<BattleRewardSystem>();
            _rewardModel = this.GetModel<BattleRewardModel>();
        }

        /// <summary>注册敌人控制器引用（由 EnemyController 在初始化时调用）</summary>
        public void RegisterEnemy(EnemyController enemy)
        {
            _enemyController = enemy;
        }

        /// <summary>启动战斗，初始化数据并发牌</summary>
        public void StartBattle(
            DeckPresetData deckPreset,
            EnemyData enemyData,
            BattleRewardConfigData rewardConfig,
            int playerMaxHp = 30,
            int maxEnergy = 3)
        {
            StartBattle(deckPreset, null, enemyData, rewardConfig, playerMaxHp, maxEnergy);
        }

        public void StartBattle(
            DeckPresetData deckPreset,
            MonsterListData monsterList,
            EnemyData enemyData,
            BattleRewardConfigData rewardConfig,
            int playerMaxHp = 30,
            int maxEnergy = 3)
        {
            _lastDeckPreset = deckPreset;
            _lastMonsterList = monsterList;
            _lastEnemyData = enemyData;
            _lastRewardConfig = rewardConfig;
            _lastPlayerMaxHp = playerMaxHp;
            _lastMaxEnergy = maxEnergy;

            var deckCards = deckPreset.BuildCardList();
            _cardSystem.Shuffle(deckCards);

            ResetPlayerState(deckCards, playerMaxHp, maxEnergy, rewardConfig);

            MonsterStageConfig firstMonster = GetMonsterConfig(0);
            EnemyData firstEnemy = firstMonster != null ? firstMonster.EnemyData : enemyData;
            int maxPlayRounds = firstMonster != null ? firstMonster.MaxPlayRounds : 2;

            if (firstEnemy == null)
            {
                Debug.LogWarning("[BattleSystem] 没有可用的怪物配置，战斗无法开始。");
                _battleModel.IsBattleOver = true;
                this.SendEvent(new BattleEndedEvent { PlayerWon = false });
                return;
            }

            this.SendEvent(new BattleStartedEvent
            {
                PlayerMaxHp = playerMaxHp,
                EnemyMaxHp = firstEnemy.MaxHp
            });

            StartMonster(0, firstEnemy, maxPlayRounds);
            StartTurn();
        }

        public void RestartBattle()
        {
            if (_lastDeckPreset == null) return;
            StartBattle(_lastDeckPreset, _lastMonsterList, _lastEnemyData, _lastRewardConfig, _lastPlayerMaxHp, _lastMaxEnergy);
        }

        void ResetPlayerState(List<CardData> deckCards, int playerMaxHp, int maxEnergy, BattleRewardConfigData rewardConfig)
        {
            _deckModel.InitDeck(deckCards);
            _battleModel.InitBattle(playerMaxHp, maxEnergy);
            _markSystem.ClearAllMarks();
            _rewardSystem.SetRewardConfig(rewardConfig);

            this.SendEvent(new HandRefreshedEvent { CardIds = new List<string>() });
            this.SendEvent<SlotEffectsResolvedEvent>();
            this.SendEvent(new DrawPileChangedEvent { Count = _deckModel.DrawPile.Count });
            this.SendEvent(new DiscardPileChangedEvent { Count = _deckModel.DiscardPile.Count });
            this.SendEvent(new PlayerHpChangedEvent
            {
                CurrentHp = _battleModel.PlayerHp.Value,
                MaxHp = _battleModel.PlayerMaxHp
            });
            this.SendEvent(new EnergyChangedEvent
            {
                CurrentEnergy = _battleModel.CurrentEnergy.Value,
                MaxEnergy = _battleModel.MaxEnergy.Value
            });
            this.SendEvent(new RedrawCountChangedEvent
            {
                Remaining = _battleModel.RedrawsRemaining,
                Max = _battleModel.RedrawsPerTurn
            });
            this.SendEvent(new MonsterPlayRoundCountChangedEvent
            {
                CurrentRound = 0,
                MaxCount = 0
            });
        }

        MonsterStageConfig GetMonsterConfig(int monsterIndex)
        {
            if (_lastMonsterList == null || _lastMonsterList.Monsters == null) return null;
            if (monsterIndex < 0 || monsterIndex >= _lastMonsterList.Monsters.Count) return null;
            MonsterStageConfig config = _lastMonsterList.Monsters[monsterIndex];
            return config != null && config.EnemyData != null ? config : null;
        }

        int GetMonsterCount()
        {
            if (_lastMonsterList == null || _lastMonsterList.Monsters == null || _lastMonsterList.Monsters.Count == 0)
                return _lastEnemyData != null ? 1 : 0;
            return _lastMonsterList.Monsters.Count;
        }

        void StartMonster(int monsterIndex, EnemyData enemyData, int maxPlayRounds)
        {
            int monsterCount = GetMonsterCount();
            _battleModel.StartMonster(monsterIndex, monsterCount, maxPlayRounds);

            if (_enemyController != null)
                _enemyController.InitEnemy(enemyData);

            this.SendEvent(new MonsterStartedEvent
            {
                MonsterIndex = monsterIndex,
                MonsterCount = monsterCount,
                EnemyName = enemyData.EnemyName,
                EnemyMaxHp = enemyData.MaxHp,
                MaxPlayRounds = maxPlayRounds
            });

            this.SendEvent(new MonsterPlayRoundCountChangedEvent
            {
                CurrentRound = _battleModel.CurrentMonsterPlayRounds,
                MaxCount = _battleModel.CurrentMonsterMaxPlayRounds
            });
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
            if (slotIndex < 0 || slotIndex >= BattleModel.SlotCount) return false;
            CardData card = _battleModel.PlaySlots[slotIndex];
            if (card == null) return false;

            _battleModel.PlaySlots[slotIndex] = null;
            _battleModel.CurrentEnergy.Value += card.EnergyCost;
            _deckModel.Hand.Add(card);

            int handIndex = _deckModel.Hand.Count - 1;
            this.SendEvent(new CardReturnedToHandEvent
            {
                CardId = card.CardId,
                HandIndex = handIndex,
                SourceSlotIndex = slotIndex
            });
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

        public bool TrySwapHandWithSlot(CardData handCard, int handIndex, int slotIndex)
        {
            if (_battleModel.IsBattleOver) return false;
            if (handCard == null) return false;
            if (handIndex < 0 || handIndex >= _deckModel.Hand.Count) return false;
            if (slotIndex < 0 || slotIndex >= BattleModel.SlotCount) return false;
            if (_deckModel.Hand[handIndex] != handCard) return false;

            CardData slotCard = _battleModel.PlaySlots[slotIndex];
            if (slotCard == null) return false;

            int newEnergy = _battleModel.CurrentEnergy.Value + slotCard.EnergyCost - handCard.EnergyCost;
            if (newEnergy < 0) return false;

            _deckModel.Hand[handIndex] = slotCard;
            _battleModel.PlaySlots[slotIndex] = handCard;
            _battleModel.CurrentEnergy.Value = newEnergy;

            this.SendEvent(new HandSlotSwappedEvent
            {
                HandCardId = handCard.CardId,
                SlotCardId = slotCard.CardId,
                HandIndex = handIndex,
                SlotIndex = slotIndex
            });
            this.SendEvent(new CardPlayedEvent { CardId = handCard.CardId, SlotIndex = slotIndex });
            this.SendEvent(new EnergyChangedEvent
            {
                CurrentEnergy = _battleModel.CurrentEnergy.Value,
                MaxEnergy = _battleModel.MaxEnergy.Value
            });

            return true;
        }

        /// <summary>结束当前回合，结算所有槽位效果，触发敌人行动，开始新回合</summary>
        public void EndTurn()
        {
            if (_battleModel.IsBattleOver) return;
            if (_rewardModel.HasPendingReward) return;

            this.SendEvent(new TurnEndedEvent { TurnNumber = _battleModel.TurnNumber.Value });

            ResolveSlots();

            if (_battleModel.IsBattleOver) return;
            if (_battleModel.IsCurrentMonsterDefeated)
            {
                if (_rewardSystem.TryOfferTurnReward()) return;
                ContinueAfterMonsterReward();
                return;
            }

            AddMonsterPlayRoundAndFailIfNeeded();
            if (_battleModel.IsBattleOver) return;

            ContinueAfterTurn();
        }

        public void ContinueAfterRewardIfReady()
        {
            if (_battleModel.IsBattleOver) return;
            if (_rewardModel.HasPendingReward) return;

            if (_battleModel.IsCurrentMonsterDefeated)
                ContinueAfterMonsterReward();
        }

        void ContinueAfterTurn()
        {
            _cardSystem.DiscardHand();

            EnemyTurn();

            if (_battleModel.IsBattleOver) return;

            StartTurn();
        }

        void ContinueAfterMonsterReward()
        {
            _cardSystem.DiscardHand();

            int nextMonsterIndex = _battleModel.CurrentMonsterIndex + 1;
            MonsterStageConfig nextMonster = GetMonsterConfig(nextMonsterIndex);

            if (nextMonster == null)
            {
                _battleModel.IsBattleOver = true;
                this.SendEvent(new BattleEndedEvent { PlayerWon = true });
                return;
            }

            StartMonster(nextMonsterIndex, nextMonster.EnemyData, nextMonster.MaxPlayRounds);
            StartTurn();
        }

        bool ResolveSlots()
        {
            bool resolvedAnyCard = false;
            _slotCardEffectBoosts.Clear();

            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                CardData card = _battleModel.PlaySlots[i];
                if (card == null) continue;

                resolvedAnyCard = true;

                var context = new BattleContext(
                    _battleModel,
                    _deckModel,
                    _enemyController,
                    this,
                    _markSystem,
                    i,
                    card,
                    _battleModel.GetLeftNeighbor(i),
                    _battleModel.GetRightNeighbor(i)
                );

                if (!card.CanActivateAtSlot(i))
                {
                    DiscardSlotCard(i);
                    continue;
                }

                _markSystem.ExecuteSlotMarks(i, MarkTrigger.BeforeCardEffects, context);
                _markSystem.ExecuteCardMarks(card, MarkTrigger.BeforeCardEffects, context);

                if (!_battleModel.IsBattleOver && !_battleModel.IsCurrentMonsterDefeated)
                {
                    context.SetUseCardEffectBoost(true);
                    foreach (CardEffect effect in card.Effects)
                    {
                        if (effect == null) continue;

                        effect.Execute(context);
                        if (_battleModel.IsBattleOver) break;
                        if (_battleModel.IsCurrentMonsterDefeated) break;
                    }
                    context.SetUseCardEffectBoost(false);
                }

                if (!_battleModel.IsBattleOver && !_battleModel.IsCurrentMonsterDefeated)
                {
                    _markSystem.ExecuteSlotMarks(i, MarkTrigger.AfterCardEffects, context);
                    _markSystem.ExecuteCardMarks(card, MarkTrigger.AfterCardEffects, context);
                }

                DiscardSlotCard(i);

                if (_battleModel.IsBattleOver) break;
                if (_battleModel.IsCurrentMonsterDefeated) break;
            }

            DiscardRemainingSlotCards();
            this.SendEvent<SlotEffectsResolvedEvent>();
            this.SendEvent(new DiscardPileChangedEvent { Count = _deckModel.DiscardPile.Count });

            _slotCardEffectBoosts.Clear();
            return resolvedAnyCard;
        }

        public void AddCardEffectBoost(int slotIndex, CardEffectBoost boost)
        {
            if (slotIndex < 0 || slotIndex >= BattleModel.SlotCount) return;
            _slotCardEffectBoosts.Add(new SlotCardEffectBoost(slotIndex, boost));
        }

        public int ModifyCardEffectAmount(int slotIndex, int amount)
        {
            if (amount <= 0) return amount;

            float result = amount;
            foreach (SlotCardEffectBoost slotBoost in _slotCardEffectBoosts)
            {
                if (slotBoost.SlotIndex != slotIndex) continue;
                result = slotBoost.Boost.Apply(result);
            }

            return Mathf.Max(0, Mathf.RoundToInt(result));
        }

        void AddMonsterPlayRoundAndFailIfNeeded()
        {
            _battleModel.AddCurrentMonsterPlayRound();
            this.SendEvent(new MonsterPlayRoundCountChangedEvent
            {
                CurrentRound = _battleModel.CurrentMonsterPlayRounds,
                MaxCount = _battleModel.CurrentMonsterMaxPlayRounds
            });

            if (_battleModel.CurrentMonsterMaxPlayRounds > 0
                && _battleModel.CurrentMonsterPlayRounds >= _battleModel.CurrentMonsterMaxPlayRounds)
            {
                FailBattle();
            }
        }

        void DiscardSlotCard(int slotIndex)
        {
            CardData card = _battleModel.PlaySlots[slotIndex];
            if (card == null) return;
            _deckModel.DiscardPile.Add(card);
            _battleModel.PlaySlots[slotIndex] = null;
        }

        void DiscardRemainingSlotCards()
        {
            for (int i = 0; i < BattleModel.SlotCount; i++)
                DiscardSlotCard(i);
        }

        void EnemyTurn()
        {
            // 敌人目前为木桩，无行动
            // 后续在此处调用 _enemyController 的行为接口
        }

        void FailBattle()
        {
            if (_battleModel.IsBattleOver) return;
            _battleModel.IsBattleOver = true;
            _battleModel.ClearSlots();
            this.SendEvent(new BattleEndedEvent { PlayerWon = false });
        }

        /// <summary>丢弃指定手牌索引并重新抽取，每回合限定次数</summary>
        public bool TryRedrawCards(List<int> handIndices)
        {
            if (_battleModel.IsBattleOver) return false;
            if (_battleModel.RedrawsRemaining <= 0) return false;
            if (handIndices == null || handIndices.Count == 0) return false;

            _battleModel.RedrawsRemaining--;
            _cardSystem.RedrawCards(handIndices);

            this.SendEvent(new RedrawCountChangedEvent
            {
                Remaining = _battleModel.RedrawsRemaining,
                Max = _battleModel.RedrawsPerTurn
            });

            return true;
        }

        void StartTurn()
        {
            _battleModel.TurnNumber.Value++;
            _battleModel.CurrentEnergy.Value = _battleModel.MaxEnergy.Value;
            _battleModel.RedrawsRemaining = _battleModel.RedrawsPerTurn;

            _markSystem.TickMarks();

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

            this.SendEvent(new RedrawCountChangedEvent
            {
                Remaining = _battleModel.RedrawsRemaining,
                Max = _battleModel.RedrawsPerTurn
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
                _battleModel.MarkCurrentMonsterDefeated();
                this.SendEvent<EnemyDiedEvent>();
            }
        }

        readonly struct SlotCardEffectBoost
        {
            public SlotCardEffectBoost(int slotIndex, CardEffectBoost boost)
            {
                SlotIndex = slotIndex;
                Boost = boost;
            }

            public int SlotIndex { get; }
            public CardEffectBoost Boost { get; }
        }
    }
}
