namespace Card5
{
    public class DrawCardCommand : AbstractCommand
    {
        readonly int _count;

        public DrawCardCommand(int count = 1)
        {
            _count = count;
        }

        protected override void OnExecute()
        {
            this.GetSystem<CardSystem>().DrawCards(_count);
        }
    }
}
