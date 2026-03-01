using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Gameplay.Character.Player;

namespace Gameplay.Character.Enemy
{
    public class EnemyMoveController : MoveController
    {
        [SerializeField] float _speed;
        [SerializeField] float _attackSpeed;
        [SerializeField] float _detectRadius;
        [SerializeField] float _attackRadius;

        public float AttackRadius => _attackRadius;

        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Chase = Animator.StringToHash("Chase");
        public static readonly int Patrol = Animator.StringToHash("Patrol");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Hurt = Animator.StringToHash("Hurt");
        public static readonly int Dead = Animator.StringToHash("Dead");

        public void Move()
        {
            Rigidbody.MovePosition(Rigidbody.position + Speed * Time.fixedDeltaTime * Direction);
        }

        public Vector2 DirectionToPlayer()
        {
            Vector2 playerPos = this.SendQuery(new PlayerPositionQuery());
            Vector2 direction = playerPos - Rigidbody.position;
            return direction;
        }

        public float SqrDistanceToPlayer()
        {
            return DirectionToPlayer().sqrMagnitude;
        }

        public bool FindPlayer()
        {
            Vector2 direction = DirectionToPlayer();
            if (direction.sqrMagnitude > _detectRadius * _detectRadius)
            {
                return false;
            }
            else
            {
                Face(direction);
                return true;
            }
        }

        public async UniTask AttackPlayer(CancellationToken ct)
        {
            CancellationToken combinedToken = CancellationTokenSource.CreateLinkedTokenSource(ct, this.GetCancellationTokenOnDestroy()).Token;
            Vector3 initialPosition = Rigidbody.position;
            Vector2 playerPosition = this.SendQuery(new PlayerPositionQuery());
            while (Vector2.Distance(playerPosition, Rigidbody.position) > 0.1f)
            {
                combinedToken.ThrowIfCancellationRequested();
                Vector2 velocity = (playerPosition - Rigidbody.position).normalized * _attackSpeed;
                Rigidbody.MovePosition(Rigidbody.position + velocity * Time.fixedDeltaTime);
                await UniTask.WaitForFixedUpdate(combinedToken);
            }


            while (Vector2.Distance(initialPosition, Rigidbody.position) > 0.1f)
            {
                combinedToken.ThrowIfCancellationRequested();
                Rigidbody.MovePosition(Vector2.Lerp(Rigidbody.position, initialPosition, _attackSpeed * Time.fixedDeltaTime));
                await UniTask.WaitForFixedUpdate(combinedToken);
            }

            Rigidbody.MovePosition(initialPosition);
        }

        public bool LosePlayer()
        {
            return SqrDistanceToPlayer() > _detectRadius * _detectRadius;
        }

        protected override void OnInit()
        {
            base.OnInit();
            Speed = _speed;
        }
    }
}
