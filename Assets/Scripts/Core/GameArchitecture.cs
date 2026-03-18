namespace Card5
{
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        protected override void Init()
        {
            RegisterModel(new BattleModel());
            RegisterModel(new DeckModel());
            RegisterModel(new MarkModel());

            RegisterSystem(new CardSystem());
            RegisterSystem(new MarkSystem());
            RegisterSystem(new BattleSystem());
        }
    }
}
