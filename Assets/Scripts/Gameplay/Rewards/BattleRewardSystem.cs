using System.Collections.Generic;
using Card5.Gameplay.Events;
using UnityEngine;

namespace Card5
{
    public class BattleRewardSystem : AbstractSystem
    {
        BattleRewardModel _rewardModel;
        BattleModel _battleModel;
        DeckModel _deckModel;
        CardSystem _cardSystem;
        BattleRewardConfigData _rewardConfig;

        protected override void OnInit()
        {
            _rewardModel = this.GetModel<BattleRewardModel>();
            _battleModel = this.GetModel<BattleModel>();
            _deckModel = this.GetModel<DeckModel>();
            _cardSystem = this.GetSystem<CardSystem>();
        }

        public void SetRewardConfig(BattleRewardConfigData rewardConfig)
        {
            _rewardConfig = rewardConfig;
            _rewardModel.Clear();
        }

        public bool TryOfferTurnReward()
        {
            if (_rewardModel.HasPendingReward) return true;
            if (_rewardConfig == null || _rewardConfig.RewardGroups.Count == 0) return false;

            List<BattleRewardOffer> offers = BuildOffers();
            if (offers.Count == 0) return false;

            _rewardModel.SetPendingOffers(offers);

            this.SendEvent(new BattleRewardOfferedEvent
            {
                RewardId = _rewardModel.CurrentRewardId,
                Offers = _rewardModel.PendingOffers
            });

            return true;
        }

        public bool ClaimReward(string offerId, string optionId)
        {
            if (!_rewardModel.HasPendingReward) return false;

            BattleRewardOffer offer = FindOffer(offerId);
            if (offer == null) return false;

            BattleRewardOption option = FindOption(offer, optionId);
            if (option == null) return false;

            ApplyReward(option);

            _rewardModel.RemoveOffer(offerId);

            this.SendEvent(new BattleRewardOptionClaimedEvent
            {
                RewardId = _rewardModel.CurrentRewardId,
                OfferId = offerId,
                OptionId = optionId,
                RewardType = option.RewardType,
                CardId = option.Card != null ? option.Card.CardId : string.Empty,
                RemainingOffers = _rewardModel.PendingOffers
            });

            if (!_rewardModel.HasPendingReward)
            {
                this.SendEvent(new BattleRewardCompletedEvent
                {
                    RewardId = _rewardModel.CurrentRewardId
                });
            }

            return true;
        }

        List<BattleRewardOffer> BuildOffers()
        {
            var offers = new List<BattleRewardOffer>();

            for (int i = 0; i < _rewardConfig.RewardGroups.Count; i++)
            {
                BattleRewardGroupConfig groupConfig = _rewardConfig.RewardGroups[i];
                BattleRewardOffer offer = BuildOffer(groupConfig, i);
                if (offer != null)
                    offers.Add(offer);
            }

            return offers;
        }

        BattleRewardOffer BuildOffer(BattleRewardGroupConfig groupConfig, int groupIndex)
        {
            switch (groupConfig.RewardType)
            {
                case BattleRewardType.Card:
                    return BuildCardOffer(groupConfig, groupIndex);
                default:
                    Debug.LogWarning($"[BattleRewardSystem] Reward type {groupConfig.RewardType} is not implemented.");
                    return null;
            }
        }

        BattleRewardOffer BuildCardOffer(BattleRewardGroupConfig groupConfig, int groupIndex)
        {
            List<CardRewardCandidate> candidates = BuildCardCandidates(groupConfig);

            if (candidates.Count == 0) return null;

            int count = Mathf.Min(groupConfig.ChoiceCount, candidates.Count);
            var options = new List<BattleRewardOption>(count);

            for (int i = 0; i < count; i++)
            {
                CardData card = TakeWeightedRandomCard(candidates);
                string optionId = $"card_{groupIndex}_{i}";
                options.Add(new BattleRewardOption(optionId, BattleRewardType.Card, card));
            }

            string offerId = $"offer_{_rewardModel.CurrentRewardId + 1}_{groupIndex}";
            return new BattleRewardOffer(offerId, BattleRewardType.Card, options);
        }

        List<CardRewardCandidate> BuildCardCandidates(BattleRewardGroupConfig groupConfig)
        {
            if (groupConfig.CardLibrary != null)
                return BuildCardLibraryCandidates(groupConfig.CardLibrary);

            var candidates = new List<CardRewardCandidate>();
            var addedCards = new HashSet<CardData>();

            foreach (CardData card in groupConfig.CardPool)
            {
                if (card == null || !addedCards.Add(card)) continue;
                candidates.Add(new CardRewardCandidate(card, 1));
            }

            return candidates;
        }

        List<CardRewardCandidate> BuildCardLibraryCandidates(CardLibraryData cardLibrary)
        {
            var candidates = new List<CardRewardCandidate>();
            var addedCards = new HashSet<CardData>();
            var context = new CardUnlockContext(_battleModel, _deckModel);

            foreach (CardLibraryEntry entry in cardLibrary.Entries)
            {
                if (entry == null || entry.Card == null) continue;
                if (!entry.IsUnlocked(context)) continue;
                if (!addedCards.Add(entry.Card)) continue;

                candidates.Add(new CardRewardCandidate(entry.Card, entry.Weight));
            }

            return candidates;
        }

        CardData TakeWeightedRandomCard(List<CardRewardCandidate> candidates)
        {
            int totalWeight = 0;
            foreach (CardRewardCandidate candidate in candidates)
                totalWeight += Mathf.Max(1, candidate.Weight);

            int roll = Random.Range(0, totalWeight);
            int currentWeight = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                currentWeight += Mathf.Max(1, candidates[i].Weight);
                if (roll >= currentWeight) continue;

                CardData card = candidates[i].Card;
                candidates.RemoveAt(i);
                return card;
            }

            CardData fallback = candidates[0].Card;
            candidates.RemoveAt(0);
            return fallback;
        }

        BattleRewardOffer FindOffer(string offerId)
        {
            foreach (BattleRewardOffer offer in _rewardModel.PendingOffers)
            {
                if (offer.OfferId == offerId)
                    return offer;
            }

            return null;
        }

        BattleRewardOption FindOption(BattleRewardOffer offer, string optionId)
        {
            foreach (BattleRewardOption option in offer.Options)
            {
                if (option.OptionId == optionId)
                    return option;
            }

            return null;
        }

        void ApplyReward(BattleRewardOption option)
        {
            switch (option.RewardType)
            {
                case BattleRewardType.Card:
                    if (option.Card != null)
                        _cardSystem.AddCardToDeck(option.Card);
                    break;
                default:
                    Debug.LogWarning($"[BattleRewardSystem] Reward type {option.RewardType} is not implemented.");
                    break;
            }
        }

        class CardRewardCandidate
        {
            public CardRewardCandidate(CardData card, int weight)
            {
                Card = card;
                Weight = weight;
            }

            public CardData Card { get; }
            public int Weight { get; }
        }
    }
}
