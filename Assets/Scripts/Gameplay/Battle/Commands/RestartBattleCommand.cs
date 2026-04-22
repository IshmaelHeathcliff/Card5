namespace Card5
{
    public class RestartBattleCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetSystem<BattleSystem>().RestartBattle();
        }
    }
}
