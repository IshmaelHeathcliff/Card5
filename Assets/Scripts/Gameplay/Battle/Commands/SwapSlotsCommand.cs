namespace Card5
{
    /// <summary>交换或移动两个槽位上的卡牌</summary>
    public class SwapSlotsCommand : AbstractCommand<bool>
    {
        readonly int _fromSlot;
        readonly int _toSlot;

        public SwapSlotsCommand(int fromSlot, int toSlot)
        {
            _fromSlot = fromSlot;
            _toSlot = toSlot;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<BattleSystem>().TrySwapSlots(_fromSlot, _toSlot);
        }
    }
}
