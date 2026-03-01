using System.Collections.Generic;
using Core;
using Core.Pool;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gameplay.Items
{
    public class ItemUIPool : MonoBehaviour, IAsyncObjectPool<ItemUI>
    {
        [SerializeField] AssetReferenceGameObject _itemUIReference;
        [SerializeField] AssetReferenceGameObject _currentItemUIReference;
        [SerializeField] int _initialSize = 10;
        [SerializeField] int _maxSize = 1000;

        AsyncOperationHandle<GameObject> _itemUIHandle;
        AsyncOperationHandle<GameObject> _currentItemUIHandle;

        readonly Stack<ItemUI> _pool = new();

        Transform _itemsHolder;

        Transform ItemsHolder
        {
            get
            {
                if (_itemsHolder == null)
                {
                    _itemsHolder = InitItemsHolder();
                }

                return _itemsHolder;
            }

        }

        int Count => _pool.Count;

        Transform InitItemsHolder()
        {
            var t = (RectTransform)transform.Find("ItemPool");
            if (t == null)
            {
                t = new GameObject("ItemPool").AddComponent<RectTransform>();
                t.SetParent(transform, false);
            }
            t.SetAsFirstSibling();

            return t;
        }

        public async UniTask<CurrentItemUI> GetNewCurrentItemUI()
        {
            var obj = await Addressables.InstantiateAsync(_currentItemUIReference, transform)
                                        .ToUniTask(cancellationToken: GlobalCancellation.GetCombinedTokenSource(this).Token);
            return obj.GetOrAddComponent<CurrentItemUI>();
        }

        async UniTask<ItemUI> CreatObject()
        {
            var obj = await Addressables.InstantiateAsync(_itemUIReference, ItemsHolder)
                                        .ToUniTask(cancellationToken: GlobalCancellation.GetCombinedTokenSource(this).Token);
            obj.SetActive(false);
            return obj.GetOrAddComponent<ItemUI>();
        }

        public async UniTask<ItemUI> Allocate()
        {
            ItemUI itemUI;
            if (Count > 0)
            {
                itemUI = _pool.Pop();
            }
            else
            {
                itemUI = await CreatObject();
            }
            itemUI.gameObject.SetActive(true);
            return itemUI;
        }

        public void Recycle(ItemUI obj)
        {
            obj.Item = null;
            if (Count > _maxSize)
            {
                Addressables.ReleaseInstance(obj.gameObject);
                return;
            }

            obj.transform.SetParent(ItemsHolder);
            obj.gameObject.SetActive(false);
            _pool.Push(obj);
        }

        async UniTaskVoid InitPool()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                _pool.Push(await CreatObject());
            }
        }

        void OnEnable()
        {
            _itemUIHandle = Addressables.LoadAssetAsync<GameObject>(_itemUIReference);
            _currentItemUIHandle = Addressables.LoadAssetAsync<GameObject>(_currentItemUIReference);
        }

        void Start()
        {
            InitPool().Forget();

        }

        void OnDisable()
        {
            Addressables.Release(_itemUIHandle);
            Addressables.Release(_currentItemUIHandle);
        }
    }
}
