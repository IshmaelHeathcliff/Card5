using Core;
using Cysharp.Threading.Tasks;
namespace Gameplay.Character.Enemy
{
    public class EnemyHurtState : EnemyState
    {
        public EnemyHurtState(FSM<EnemyStateID> fsm, EnemyController target) : base(fsm, target)
        {
        }

        protected override bool OnCondition()
        {
            return FSM.CurrentStateId is not EnemyStateID.Dead;
        }

        protected async override void OnEnter()
        {
            Target.Damageable.IsDamageable = false;
            MoveController.Stop();

            if (Target.Damageable.Health.CurrentValue <= 0)
            {
                FSM.ChangeState(EnemyStateID.Dead);
            }
            else
            {
                await MoveController.PlayAnimation(EnemyMoveController.Hurt);
                FSM.ChangeState(EnemyStateID.Idle);
            }

        }

        protected override void OnExit()
        {
            Target.Damageable.IsDamageable = true;
        }
    }
}
