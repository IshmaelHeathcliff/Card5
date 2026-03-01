using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Gameplay.Character.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Gameplay.Character.Enemy
{
    public class EnemyFactory : MonoBehaviour, IController
    {
        [SerializeField] AssetReferenceGameObject _enemyReference;
        [Header("生成信息")]
        [SerializeField] int _maxCount;
        [SerializeField] float _generateGap;
        [SerializeField] float _minDistance;
        [SerializeField] float _maxDistance;

        /// <summary>
        /// 创建EnemyModel，将EnemyModel的相关依赖注入到EnemiesModel中
        /// </summary>
        async UniTask CreateEnemy(Vector3 position, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var obj = await Addressables.InstantiateAsync(_enemyReference, transform).ToUniTask(cancellationToken: ct);
            obj.transform.position = position;
        }

        async UniTask ProduceEnemies()
        {
            CancellationToken ct = GlobalCancellation.GetCombinedTokenSource(this).Token;

            try
            {
                while (true)
                {
                    if (transform.childCount < _maxCount)
                    {
                        await CreateEnemy(GetRandomPosition(), ct);
                    }

                    await UniTask.Delay((int)(_generateGap * 1000), cancellationToken: ct); // ms
                }
            }
            catch (OperationCanceledException)
            {
                // Debug.Log("EnemyFactory is canceled");
            }
        }

        Vector2 GetRandomPosition()
        {
            Vector2 playerPosition = this.SendQuery(new PlayerPositionQuery());
            float angle = Random.Range(0, 2 * Mathf.PI);
            var randomDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 randomPosition = playerPosition + randomDirection * Random.Range(_minDistance, _maxDistance);
            return randomPosition;
        }

        void Start()
        {
            ProduceEnemies().Forget();
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
