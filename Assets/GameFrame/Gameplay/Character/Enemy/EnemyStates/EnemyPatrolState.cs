using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Character.Enemy
{
    public class EnemyPatrolState : EnemyState
    {
        bool _quitting;
        public EnemyPatrolState(FSM<EnemyStateID> fsm, EnemyController target) : base(fsm, target)
        {
        }

        protected override bool OnCondition()
        {
            return FSM.CurrentStateId is EnemyStateID.Idle;
        }

        protected override void OnEnter()
        {
            _quitting = false;
            MoveController.PlayAnimation(EnemyMoveController.Patrol).Forget();
            ChangeDirection().Forget();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnFixedUpdate()
        {
            if (MoveController.FindPlayer())
            {
                FSM.ChangeState(EnemyStateID.Chase);
                return;
            }

            MoveController.Move();
        }

        protected override void OnExit()
        {
            _quitting = true;
        }

        Vector2 RandomDirection()
        {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            return new Vector2(x, y);
        }

        async UniTaskVoid ChangeDirection()
        {
            while (!_quitting)
            {
                MoveController.Face(RandomDirection());
                await UniTask.Delay((int)(Random.Range(1f, 3f) * 1000));
            }
        }
    }
}
