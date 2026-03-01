using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Damage.Attackers
{
    public class SelfAttacker : Attacker
    {
        Collider2D _collider;

        CancellationTokenSource _cts;


        void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        void Start()
        {
            _cts = GlobalCancellation.GetCombinedTokenSource(this);
            Attack(_cts.Token).Forget();
        }

        protected override UniTask Play(CancellationToken cancellationToken)
        {
            // _collider.enabled = true;
            // TODO: 添加攻击特效
            return UniTask.CompletedTask;
        }

        public override async UniTaskVoid Attack(CancellationToken cancellationToken)
        {
            await Play(cancellationToken);
        }


        public override async UniTaskVoid Cancel()
        {
            _cts.Cancel();
            await UniTask.CompletedTask;
        }


        void OnTriggerEnter2D(Collider2D other)
        {
            if (!AttackerController.CanAttack)
            {
                return;
            }

            Damageable damageable = other.GetComponent<Damageable>();

            if (damageable == null || !damageable.CompareTag(TargetTag))
            {
                return;
            }


            var damage = new AttackDamage(this, damageable, Keywords, DamageType.Simple, Damage.BaseValue, 1, 1);
            damage.Apply();
        }

    }
}
