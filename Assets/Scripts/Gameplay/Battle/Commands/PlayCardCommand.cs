namespace Card5
{
    public class PlayCardCommand : AbstractCommand<bool>
    {
        readonly CardData _card;
        readonly int _slotIndex;

        public PlayCardCommand(CardData card, int slotIndex)
        {
            _card = card;
            _slotIndex = slotIndex;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<BattleSystem>().TryPlayCard(_card, _slotIndex);
        }
    }
}
