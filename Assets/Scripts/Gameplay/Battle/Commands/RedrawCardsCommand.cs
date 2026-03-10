using System.Collections.Generic;

namespace Card5
{
    public class RedrawCardsCommand : AbstractCommand<bool>
    {
        readonly List<int> _handIndices;

        public RedrawCardsCommand(List<int> handIndices)
        {
            _handIndices = handIndices;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<BattleSystem>().TryRedrawCards(_handIndices);
        }
    }
}
