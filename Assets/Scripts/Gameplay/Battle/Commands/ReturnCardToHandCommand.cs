namespace Card5
{
    /// <summary>将已放入槽位的卡牌撤回到手牌（取消出牌）</summary>
    public class ReturnCardToHandCommand : AbstractCommand<bool>
    {
        readonly int _slotIndex;

        public ReturnCardToHandCommand(int slotIndex)
        {
            _slotIndex = slotIndex;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<BattleSystem>().TryReturnCardFromSlot(_slotIndex);
        }
    }
}
