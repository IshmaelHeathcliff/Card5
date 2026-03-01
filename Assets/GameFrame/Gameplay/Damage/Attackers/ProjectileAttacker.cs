using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using Gameplay.Character;
using Gameplay.Items;
using Gameplay.Stat;

namespace Gameplay.Damage.Attackers
{
    public interface IProjectileAttacker : IAttacker
    {
        IStat ProjectileSpeed { get; }
    }

    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class ProjectileAttacker : Attacker, IProjectileAttacker
    {
        [SerializeField] float _collisionRadius = 1f;
        [SerializeField] float _rotateSpeed;
        [SerializeField] float _randomDirectionFactor = 0.5f;

        public IStat ProjectileSpeed => AttackSkill.ProjectileSpeed;
        public IStat ChainCount => AttackSkill.ChainCount;
        public IStat PenetrateCount => AttackSkill.PenetrateCount;
        public IStat SplitCount => AttackSkill.SplitCount;
        public bool CanReturn => AttackSkill.CanReturn;
        public bool IsTargetLocked => AttackSkill.IsTargetLocked;


        Collider2D _collider;
        Rigidbody2D _rigidbody;
        SpriteRenderer _renderer;

        CancellationTokenSource _cts;

        readonly List<string> _damaged = new();
        int _penetrateLeft;
        int _chainLeft;
        float _durationLeft;

        bool _isTargetLocked;
        bool _isReturning;
        bool _isFreeze;


        void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }


        void Start()
        {
            _penetrateLeft = (int)PenetrateCount.Value;
            _chainLeft = (int)ChainCount.Value;
            _isTargetLocked = IsTargetLocked;
            _cts = GlobalCancellation.GetCombinedTokenSource(this);
            Attack(_cts.Token).Forget();
        }

        void ResetDuration()
        {
            _durationLeft = Duration.Value;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Damageable damageable) || !damageable.IsDamageable)
            {
                // Debug.Log("not damageable");
                return;
            }

            if (_isTargetLocked && Target != null && !Target.GetComponentInChildren<Damageable>().Equals(damageable))
            {
                // Debug.Log("not target");
                return;
            }

            if (!damageable.CompareTag(TargetTag))
            {
                // Debug.Log("not target tag");
                return;
            }

            if (_damaged.Contains(damageable.ID)) // 不能对同一个敌人造成多次伤害
            {
                // Debug.Log("already damaged");
                return;
            }

            ApplyDamage(damageable);
        }

        void ApplyDamage(IDamageable damageable)
        {
            _damaged.Add(damageable.ID);

            var damage = new AttackDamage(this, damageable, Keywords, DamageType.Simple, Damage.BaseValue, 1f, 1f);
            // Debug.Log($"Base Damage: {Damage.BaseValue}");
            damage.Apply();
            // Debug.Log($"Skill Damage: {Damage.GetValueByKeywords(Keywords)}");

            if (_isReturning)
            {
                return;
            }

            // 如果目标未锁定，且有穿透次数，则穿透
            if (!_isTargetLocked && _penetrateLeft > 0)
            {
                _penetrateLeft--;
                return;
            }
            else if (_chainLeft > 0)
            {
                _chainLeft--;
                if (Chain())
                {
                    return;
                }
            }
            else if (SplitCount.Value > 0)
            {
                Split();
                return;
            }
            else if (CanReturn)
            {
                Return();
                return;
            }

            Cancel().Forget();
        }

        void Split()
        {
            return;
        }

        bool Chain()
        {
            Transform newTarget = this.GetSystem<PositionQuerySystem>().QueryClosest(TargetTag, transform.position, _damaged);
            if (newTarget == null)
            {
                // Debug.LogError("Chain failed");
                return false;
            }

            Target = newTarget;
            _isTargetLocked = true;

            Direction = (Target.position - transform.position).normalized;
            transform.right = Direction;
            return true;
        }

        void Return()
        {
            _isReturning = true;
            Target = AttackerController.CharacterController.CharacterModel.Transform;
            Direction = Target.position - transform.position;
            transform.right = Direction;
            _isTargetLocked = true;
        }

        void Move()
        {
            if (_isFreeze)
            {
                return;
            }

            if (_isTargetLocked)
            {
                if (Target == null)
                {
                    Target = this.GetSystem<PositionQuerySystem>().QueryClosest(TargetTag, transform.position, _damaged);
                    if (Target == null)
                    {
                        Debug.LogError("Can't find target");
                        return;
                    }
                }

                Direction = ((Vector2)(Target.position - transform.position)).normalized;
                if ((transform.position - Target.position).sqrMagnitude > _collisionRadius * _collisionRadius)
                {
                    if (_collider.enabled)
                    {
                        _collider.enabled = false;
                    }
                }
                else
                {
                    if (!_collider.enabled)
                    {
                        _collider.enabled = true;
                    }
                }
            }

            _rigidbody.MovePosition(_rigidbody.position + ProjectileSpeed.Value * Time.fixedDeltaTime * Direction);
            _rigidbody.MoveRotation(_rigidbody.rotation + _rotateSpeed * 360 * Time.fixedDeltaTime);
        }

        protected override async UniTask Play(CancellationToken cancellationToken)
        {
            if (Target != null)
            {
                Direction = Target.position - transform.position;
            }
            else
            {
                Direction = transform.right;
            }

            // 方向产生一定随机性
                float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            var randomDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Direction = (Direction + _randomDirectionFactor * Direction.magnitude * randomDirection).normalized;
            transform.right = Direction;

            ResetDuration();

            while (!_isReturning && _durationLeft > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Move();
                _durationLeft -= Time.fixedDeltaTime;
                await UniTask.WaitForFixedUpdate(cancellationToken);
            }
        }

        public override async UniTaskVoid Attack(CancellationToken cancellationToken)
        {
            // TODO 暂时性处理 WoodOnUse
            if (WoodOnUse.Value > 0)
            {
                if (AttackerController.CharacterController.CharacterModel is IHasResources resourceModel)
                {
                    this.GetSystem<ResourceSystem>().AcquireResource(ResourceType.Wood, (int)WoodOnUse.Value, resourceModel);
                }
            }

            try
            {
                await Play(cancellationToken);

                if (CanReturn && !_isReturning)
                {
                    Return();
                }

                if (_isReturning)
                {
                    while (Vector2.SqrMagnitude(Target.position - transform.position) > 0.1f)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await UniTask.WaitForFixedUpdate(cancellationToken);
                    }
                }

                Cancel().Forget();
            }
            catch (OperationCanceledException)
            {

            }

        }

        public override async UniTaskVoid Cancel()
        {
            _cts.Cancel();
            if (this != null)
            {
                AttackerController?.RemoveAttacker(this);
                Addressables.ReleaseInstance(gameObject);
            }

            await UniTask.CompletedTask;
        }
    }
}
