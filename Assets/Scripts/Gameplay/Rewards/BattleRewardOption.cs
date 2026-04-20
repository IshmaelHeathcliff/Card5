namespace Card5
{
    public class BattleRewardOption
    {
        public BattleRewardOption(string optionId, BattleRewardType rewardType, CardData card)
        {
            OptionId = optionId;
            RewardType = rewardType;
            Card = card;
        }

        public string OptionId { get; }
        public BattleRewardType RewardType { get; }
        public CardData Card { get; }
    }
}
