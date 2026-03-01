using System.Collections.Generic;
using Gameplay.Status;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Gameplay.Character.Player;


namespace UI
{
    public class PlayerStatusUIController : StatusUIController
    {
        protected override void SetStatusContainer()
        {
            StatusContainer = this.GetModel<PlayersModel>().Current.StatusContainer;
        }
    }
}
