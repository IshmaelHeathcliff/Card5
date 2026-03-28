namespace Card5
{
    public class AddCardToDeckCommand : AbstractCommand
    {
        readonly CardData _card;

        public AddCardToDeckCommand(CardData card)
        {
            _card = card;
        }

        protected override void OnExecute()
        {
            this.GetSystem<CardSystem>().AddCardToDeck(_card);
        }
    }
}
