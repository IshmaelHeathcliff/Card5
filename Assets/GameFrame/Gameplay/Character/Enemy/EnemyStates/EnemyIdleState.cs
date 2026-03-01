using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Character.Enemy
{
    public class EnemyIdleState : EnemyState
    {
        public EnemyIdleState(FSM<EnemyStateID> fsm, EnemyController target) : base(fsm, target)
        {
        }

        protected override bool OnCondition()
        {
            return FSM.CurrentStateId is not EnemyStateID.Dead;
        }

        protected async override void OnEnter()
        {
            MoveController.Stop();
            await MoveController.PlayAnimation(EnemyMoveController.Idle);
            FSM.ChangeState(EnemyStateID.Patrol);
        }
    }
}
