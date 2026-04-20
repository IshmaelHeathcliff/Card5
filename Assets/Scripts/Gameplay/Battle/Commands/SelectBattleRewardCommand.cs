namespace Card5
{
    public class SelectBattleRewardCommand : AbstractCommand<bool>
    {
        readonly string _offerId;
        readonly string _optionId;

        public SelectBattleRewardCommand(string offerId, string optionId)
        {
            _offerId = offerId;
            _optionId = optionId;
        }

        protected override bool OnExecute()
        {
            bool success = this.GetSystem<BattleRewardSystem>().ClaimReward(_offerId, _optionId);
            if (success)
                this.GetSystem<BattleSystem>().ContinueAfterRewardIfReady();

            return success;
        }
    }
}
