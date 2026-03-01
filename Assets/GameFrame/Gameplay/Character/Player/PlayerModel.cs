using System.Collections.Generic;
using Gameplay.Items;

namespace Gameplay.Character.Player
{
    public class PlayerModel : CharacterModel, IHasResources
    {
        public IResourceContainer Resources { get; } = new ResourceContainer();
    }

    public class PlayersModel : CharactersModel<PlayerModel>
    {
        protected override void OnInit()
        {
            Current = AddModel("player");
        }
    }
}
