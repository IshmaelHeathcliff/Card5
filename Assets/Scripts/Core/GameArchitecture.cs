namespace Card5
{
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        protected override void Init()
        {
            RegisterModel(new BattleModel());
            RegisterModel(new DeckModel());

            RegisterSystem(new CardSystem());
            RegisterSystem(new BattleSystem());
        }
    }
}
