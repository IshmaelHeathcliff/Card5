using Core;
using Cysharp.Threading.Tasks;

namespace Gameplay.Character.Enemy
{
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(FSM<EnemyStateID> fsm, EnemyController target) : base(fsm, target)
        {
        }

        float _attackRadius;



        void CheckPlayer()
        {
            if (MoveController.SqrDistanceToPlayer() < _attackRadius * _attackRadius)
            {
                FSM.ChangeState(EnemyStateID.Attack);
            }
        }

        protected override bool OnCondition()
        {
            return FSM.CurrentStateId is EnemyStateID.Idle or EnemyStateID.Patrol;
        }

        protected override void OnEnter()
        {
            MoveController.PlayAnimation(EnemyMoveController.Chase).Forget();
            _attackRadius = MoveController.AttackRadius;

        }

        protected override void OnUpdate()
        {
        }

        protected override void OnFixedUpdate()
        {
            if (MoveController.LosePlayer())
            {
                FSM.ChangeState(EnemyStateID.Idle);
                return;
            }

            MoveController.FindPlayer();
            MoveController.Move();
            CheckPlayer();
        }
    }
}
