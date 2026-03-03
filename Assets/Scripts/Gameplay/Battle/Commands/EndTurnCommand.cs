namespace Card5
{
    public class EndTurnCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetSystem<BattleSystem>().EndTurn();
        }
    }
}
