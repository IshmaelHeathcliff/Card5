using System.Collections.Generic;

namespace Card5
{
    public class BattleRewardOffer
    {
        readonly List<BattleRewardOption> _options;

        public BattleRewardOffer(string offerId, BattleRewardType rewardType, List<BattleRewardOption> options)
        {
            OfferId = offerId;
            RewardType = rewardType;
            _options = options;
        }

        public string OfferId { get; }
        public BattleRewardType RewardType { get; }
        public IReadOnlyList<BattleRewardOption> Options => _options;
    }
}
