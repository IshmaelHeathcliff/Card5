namespace Card5
{
    /// <summary>将手牌与指定槽位上的卡牌交换</summary>
    public class SwapHandWithSlotCommand : AbstractCommand<bool>
    {
        readonly CardData _handCard;
        readonly int _handIndex;
        readonly int _slotIndex;

        public SwapHandWithSlotCommand(CardData handCard, int handIndex, int slotIndex)
        {
            _handCard = handCard;
            _handIndex = handIndex;
            _slotIndex = slotIndex;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<BattleSystem>().TrySwapHandWithSlot(_handCard, _handIndex, _slotIndex);
        }
    }
}
