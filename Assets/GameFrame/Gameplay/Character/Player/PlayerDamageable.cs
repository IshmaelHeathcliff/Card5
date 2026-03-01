using System;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Damage;
using UnityEngine;

namespace Gameplay.Character.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerDamageable : Damageable
    {
        [SerializeField] float _invincibleTime = 1f;

        protected override void OnInit()
        {
            base.OnInit();
            OnHurt = new EasyEvent();
            OnDeath = new EasyEvent();
        }


        void Start()
        {
            SetStats(CharacterController.CharaterStats);

            OnHurt.Register(() => { }).UnRegisterWhenDisabled(this);
            OnDeath.Register(() => Dead().Forget()).UnRegisterWhenDisabled(this);
        }

        public override void TakeDamage(float damage)
        {
            if (!IsDamageable)
            {
                return;
            }

            Health.ChangeCurrentValue(-damage);
            // Debug.Log($"TakeDamage: {damage}, Left Health: {Health.CurrentValue}");
            OnHurt.Trigger();

            if (Health.CurrentValue <= 0)
            {
                OnDeath.Trigger();
            }

            Invincible().Forget();
        }

        async UniTaskVoid Invincible()
        {
            IsDamageable = false;
            await UniTask.Delay(TimeSpan.FromSeconds(_invincibleTime), cancellationToken: GlobalCancellation.GetCombinedTokenSource(this).Token);
            IsDamageable = true;
        }

        async UniTaskVoid Dead()
        {
            IsDamageable = false;
            await UniTask.Delay((int)(1000 * 0.5f));
            (CharacterController as PlayerController)?.Respawn();
            await UniTask.Delay((int)(1000 * 0.5f));
            IsDamageable = true;
        }
    }
}
