using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Character
{
    public interface IMoveController : ICharacterControlled
    {
        float Speed { get; set; }
        Transform Transform { get; }
        Vector2 Direction { get; set; }
        Vector2 Position { get; set; }
        void Face(Vector2 direction);
        void Stop();
        void MoveTo(Vector2 position);
    }

    [RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
    public abstract class MoveController : CharacterControlled, IMoveController, IController
    {
        protected Animator Animator;
        protected Rigidbody2D Rigidbody;
        protected ICharacterModel Model => CharacterController.CharacterModel;

        static readonly int Y = Animator.StringToHash("Y");
        static readonly int X = Animator.StringToHash("X");

        public virtual float Speed
        {
            get => Model.Speed;
            set => Model.Speed = value;
        }

        public Transform Transform => transform;

        public virtual Vector2 Direction
        {
            get => Model.Direction;
            set => Model.Direction = value.normalized;
        }


        public virtual Vector2 Position
        {
            get => Model.Position;
            set => Model.Position = value;
        }

        protected override void OnInit()
        {
            Animator = GetComponentInChildren<Animator>();
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        protected override void OnDeinit()
        {

        }

        public async UniTask PlayAnimation(string stateName)
        {
            try
            {
                CancellationTokenSource cts = GlobalCancellation.GetCombinedTokenSource(this);

                Animator.Play(stateName);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: cts.Token);

                await UniTask.WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async UniTask PlayAnimation(int stateNameHash)
        {
            try
            {
                CancellationTokenSource cts = GlobalCancellation.GetCombinedTokenSource(this);

                Animator.Play(stateNameHash);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationToken: cts.Token);

                AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
                await UniTask.Delay(TimeSpan.FromSeconds(stateInfo.length), cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public virtual void Face(Vector2 direction)
        {
            Animator.SetFloat(X, direction.x);
            Animator.SetFloat(Y, direction.y);
            Direction = direction;
        }

        public virtual void Stop()
        {
            Rigidbody.linearVelocity = Vector2.zero;
        }

        public virtual void MoveTo(Vector2 position)
        {
            Position = position;
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
