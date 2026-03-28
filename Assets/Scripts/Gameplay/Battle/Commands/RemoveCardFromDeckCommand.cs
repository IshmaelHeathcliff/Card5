namespace Card5
{
    public class RemoveCardFromDeckCommand : AbstractCommand<bool>
    {
        readonly CardData _card;

        public RemoveCardFromDeckCommand(CardData card)
        {
            _card = card;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<CardSystem>().RemoveCardFromDeck(_card);
        }
    }
}
