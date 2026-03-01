using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Factory;
using Sirenix.OdinInspector;

namespace Core.Pool
{
    public class ComponentPool<T> : MonoBehaviour, IAsyncObjectPool<T> where T : Component
    {
        [SerializeField] AssetReferenceGameObject _prefabReference;
        [SerializeField] int _initialSize = 10;
        [SerializeField] int _maxSize = 100;
        readonly Stack<T> _pool = new();

        public int Count => _pool.Count;

        SingleGameObjectFactory _factory;

        async UniTask<T> CreatObject()
        {
            GameObject obj = await _factory.Create(transform);
            return obj.GetOrAddComponent<T>();
        }

        public async UniTask<T> Allocate()
        {
            T obj;
            if (Count > 0)
            {
                obj = _pool.Pop();
            }
            else
            {
                obj = await CreatObject();
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Recycle(T obj)
        {
            obj.gameObject.SetActive(false);
            if (Count > _maxSize)
            {
                _factory.Destroy(obj.gameObject);
                return;
            }
            _pool.Push(obj);
        }

        async UniTaskVoid Init()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                _pool.Push(await CreatObject());
            }
        }

        void Awake()
        {
            _factory = new SingleGameObjectFactory(_prefabReference);
        }

        void OnEnable()
        {
            Init().Forget();
        }

        void OnDisable()
        {
            foreach (T component in _pool)
            {
                _factory.Destroy(component.gameObject);
            }

            _pool.Clear();
        }

        void OnDestroy()
        {
            _factory = null;
        }
    }

    public class MultiComponentPool<T> : MonoBehaviour, IMultiAsyncObjectPool<T> where T : Component
    {
        [SerializeField] int _initialSize = 10;
        [SerializeField] int _maxSize = 100;

        readonly MultiGameObjectFactory _factory = new();

        readonly Dictionary<string, Stack<T>> _pools = new();

        public int TypeCount => _pools.Count;

        public int GetCount(string id) => _pools[id].Count;

        async UniTask<T> CreatObject(string id)
        {
            GameObject obj = await _factory.Create(id, transform);
            return obj.GetOrAddComponent<T>();
        }

        public async UniTask AddReference(string id, AssetReferenceGameObject reference)
        {
            _factory.AddReference(id, reference);
            await InitPool(id);
        }

        public async UniTask AddReference(string id, string address)
        {
            await AddReference(id, new AssetReferenceGameObject(address));
        }

        public void RemoveReference(string id)
        {
            _factory.RemoveReference(id);
            if (_pools.ContainsKey(id))
            {
                foreach (T component in _pools[id])
                {
                    _factory.Destroy(component.gameObject);
                }

                _pools[id].Clear();
                _pools.Remove(id);
            }
        }

        public async UniTask<T> Allocate(string id)
        {
            if (!_pools.ContainsKey(id))
            {
                _pools.Add(id, new Stack<T>());
            }

            T component;
            if (_pools[id].Count > 0)
            {
                component = _pools[id].Pop();
            }
            else
            {
                component = await CreatObject(id);
            }

            component.gameObject.SetActive(true);
            return component;
        }

        public void Recycle(string id, T component)
        {
            if (!_pools.ContainsKey(id))
            {
                _pools.Add(id, new Stack<T>());
            }

            component.gameObject.SetActive(false);
            if (_pools[id].Count > _maxSize)
            {
                _factory.Destroy(component.gameObject);
                return;
            }
            _pools[id].Push(component);
        }

        async UniTask InitPool(string id)
        {
            if (!_pools.ContainsKey(id))
            {
                _pools.Add(id, new Stack<T>());
            }

            for (int i = 0; i < _initialSize; i++)
            {
                _pools[id].Push(await CreatObject(id));
            }
        }

        void OnDisable()
        {
            foreach (var pool in _pools)
            {
                foreach (T component in pool.Value)
                {
                    if (component != null)
                    {
                        _factory.Destroy(component.gameObject);
                    }
                }

                pool.Value.Clear();
            }

            _pools.Clear();
        }
    }
}
