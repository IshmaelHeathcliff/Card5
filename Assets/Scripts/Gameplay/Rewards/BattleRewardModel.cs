using System.Collections.Generic;

namespace Card5
{
    public class BattleRewardModel : AbstractModel
    {
        readonly List<BattleRewardOffer> _pendingOffers = new List<BattleRewardOffer>();

        public IReadOnlyList<BattleRewardOffer> PendingOffers => _pendingOffers;
        public int CurrentRewardId { get; private set; }
        public bool HasPendingReward => _pendingOffers.Count > 0;

        protected override void OnInit()
        {
        }

        public void SetPendingOffers(List<BattleRewardOffer> offers)
        {
            _pendingOffers.Clear();
            _pendingOffers.AddRange(offers);
            CurrentRewardId++;
        }

        public bool RemoveOffer(string offerId)
        {
            int index = _pendingOffers.FindIndex(offer => offer.OfferId == offerId);
            if (index < 0) return false;
            _pendingOffers.RemoveAt(index);
            return true;
        }

        public void Clear()
        {
            _pendingOffers.Clear();
            CurrentRewardId = 0;
        }
    }
}
