using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Factory;

namespace Core.Pool
{
    public class SingleGameObjectPool : MonoBehaviour, IAsyncObjectPool<GameObject>
    {
        [SerializeField] AssetReferenceGameObject _prefabReference;
        [SerializeField] int _initialSize = 10;
        [SerializeField] int _maxSize = 100;
        readonly Stack<GameObject> _pool = new();

        public int Count => _pool.Count;

        SingleGameObjectFactory _factory;

        async UniTask<GameObject> CreatObject()
        {
            return await _factory.Create();
        }

        public async UniTask<GameObject> Allocate()
        {
            GameObject obj;
            if (Count > 0)
            {
                obj = _pool.Pop();
            }
            else
            {
                obj = await CreatObject();
            }

            obj.SetActive(true);
            return obj;
        }

        public void Recycle(GameObject obj)
        {
            obj.SetActive(false);
            if (Count > _maxSize)
            {
                _factory.Destroy(obj);
                return;
            }
            _pool.Push(obj);
        }

        async UniTaskVoid Init()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                GameObject obj = await CreatObject();
                obj.SetActive(false);
                _pool.Push(obj);
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
            foreach (GameObject obj in _pool)
            {
                _factory.Destroy(obj);
            }

            _pool.Clear();
        }

        void OnDestroy()
        {
            _factory = null;
        }
    }

    public class MultiGameObjectPool : MonoBehaviour, IMultiAsyncObjectPool<GameObject>
    {
        [SerializeField] int _initialSize = 10;
        [SerializeField] int _maxSize = 100;

        [SerializeField] MultiGameObjectFactory _factory = new();

        readonly Dictionary<string, Stack<GameObject>> _pools = new();

        public int TypeCount => _pools.Count;

        public int GetCount(string id) => _pools[id].Count;

        async UniTask<GameObject> CreatObject(string id)
        {
            return await _factory.Create(id, transform);
        }

        public void AddReference(string id, AssetReferenceGameObject reference)
        {
            _factory.AddReference(id, reference);
        }

        public void AddReference(string id, string address)
        {
            _factory.AddReference(id, address);
        }

        public void RemoveReference(string id)
        {
            _factory.RemoveReference(id);
        }

        public async UniTask<GameObject> Allocate(string id)
        {
            if (!_pools.ContainsKey(id))
            {
                _pools.Add(id, new Stack<GameObject>());
            }

            GameObject obj;
            if (_pools[id].Count > 0)
            {
                obj = _pools[id].Pop();
            }
            else
            {
                obj = await CreatObject(id);
            }

            obj.SetActive(true);
            return obj;
        }

        public void Recycle(string id, GameObject obj)
        {
            if (!_pools.ContainsKey(id))
            {
                _pools.Add(id, new Stack<GameObject>());
            }

            obj.SetActive(false);
            if (_pools[id].Count > _maxSize)
            {
                _factory.Destroy(obj);
                return;
            }
            _pools[id].Push(obj);
        }

        async UniTaskVoid Init()
        {
            foreach (string id in _factory.GetReferences())
            {
                if (!_pools.ContainsKey(id))
                {
                    _pools.Add(id, new Stack<GameObject>());
                }

                for (int i = 0; i < _initialSize; i++)
                {
                    _pools[id].Push(await CreatObject(id));
                }
            }
        }

        void OnEnable()
        {
            Init().Forget();
        }

        void OnDisable()
        {
            foreach (var pool in _pools)
            {
                foreach (GameObject obj in pool.Value)
                {
                    _factory.Destroy(obj);
                }

                pool.Value.Clear();
            }

            _pools.Clear();
        }
    }
}
