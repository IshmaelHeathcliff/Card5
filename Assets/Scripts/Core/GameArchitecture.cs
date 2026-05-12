namespace Card5
{
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        protected override void Init()
        {
            RegisterModel(new BattleModel());
            RegisterModel(new DeckModel());
            RegisterModel(new BattleRewardModel());

            RegisterSystem(new CardSystem());
            RegisterSystem(new BattleRewardSystem());
            RegisterSystem(new BattleSystem());
        }
    }
}
