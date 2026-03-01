using Core;

namespace Gameplay.Character.Enemy
{
    public enum EnemyStateID
    {
        Idle, Patrol, Chase, Attack, Hurt, Dead
    }

    public class EnemyState : AbstractState<EnemyStateID, EnemyController>
    {
        protected EnemyMoveController MoveController;
        public EnemyState(FSM<EnemyStateID> fsm, EnemyController target) : base(fsm, target)
        {
            if (target.MoveController is EnemyMoveController enemyMoveController)
            {
                MoveController = enemyMoveController;
            }
        }
    }
}