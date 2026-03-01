using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Damage.Attackers;

namespace Gameplay.Character.Enemy
{
    public class EnemyAttackerController : AttackerController
    {
        FSM<EnemyStateID> _fsm;
        public FSM<EnemyStateID> FSM => _fsm;

        AttackerSystem _attackerSystem;

        protected override void OnInit()
        {
            base.OnInit();
            _fsm = (CharacterController as IHasFSM<EnemyStateID>)?.FSM;
            _attackerSystem = this.GetSystem<AttackerSystem>();
        }
    }
}
