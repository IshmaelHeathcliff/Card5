namespace Card5
{
    public class PlayCardCommand : AbstractCommand<bool>
    {
        readonly CardData _card;
        readonly int _slotIndex;
        readonly int _handIndex;

        /// <param name="handIndex">手牌中的索引，用于区分相同 CardData 的多张牌；若 &lt; 0 则按 CardData 查找</param>
        public PlayCardCommand(CardData card, int slotIndex, int handIndex = -1)
        {
            _card = card;
            _slotIndex = slotIndex;
            _handIndex = handIndex;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<BattleSystem>().TryPlayCard(_card, _slotIndex, _handIndex);
        }
    }
}
