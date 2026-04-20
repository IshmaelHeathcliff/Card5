namespace Card5
{
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        protected override void Init()
        {
            RegisterModel(new BattleModel());
            RegisterModel(new DeckModel());
            RegisterModel(new MarkModel());
            RegisterModel(new BattleRewardModel());

            RegisterSystem(new CardSystem());
            RegisterSystem(new MarkSystem());
            RegisterSystem(new BattleRewardSystem());
            RegisterSystem(new BattleSystem());
        }
    }
}
