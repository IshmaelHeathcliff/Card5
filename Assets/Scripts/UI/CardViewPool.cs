using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Card5
{
    /// <summary>
    /// 手牌 View 对象池，预热时批量加载，取用/归还时只做 SetActive，不 Instantiate/Destroy。
    /// 挂在场景中的单例 GameObject 上，由 HandViewController 使用。
    /// </summary>
    public class CardViewPool : MonoBehaviour
    {
        [SerializeField, Required] AssetReferenceGameObject _cardViewPrefab;
        [SerializeField, Required] Transform _poolContainer;
        [SerializeField, MinValue(1)] int _initialPoolSize = 10;

        readonly Stack<CardViewController> _free = new Stack<CardViewController>();
        readonly List<AsyncOperationHandle<GameObject>> _handles = new List<AsyncOperationHandle<GameObject>>();

        bool _ready;

        public bool IsReady => _ready;

        public static CardViewPool Instance { get; private set; }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            foreach (var handle in _handles)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            _handles.Clear();
            _free.Clear();
        }

        /// <summary>在 Awake 中自动预热，确保战斗开始前对象池已就绪</summary>
        async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            await WarmUpAsync();
        }

        /// <summary>预热：批量实例化并隐藏，完成后 IsReady = true</summary>
        public async UniTask WarmUpAsync()
        {
            var tasks = new List<UniTask>();
            for (int i = 0; i < _initialPoolSize; i++)
                tasks.Add(CreateOneAsync());

            await UniTask.WhenAll(tasks);
            _ready = true;
        }

        async UniTask CreateOneAsync()
        {
            var handle = Addressables.InstantiateAsync(_cardViewPrefab, _poolContainer);
            _handles.Add(handle);
            var go = await handle;
            go.SetActive(false);
            var view = go.GetComponent<CardViewController>();
            if (view != null)
                _free.Push(view);
        }

        /// <summary>从池中取一个 View，若池空则同步扩容（不推荐，预热时应保证足够）</summary>
        public CardViewController Rent(Transform parent)
        {
            if (_free.Count == 0)
            {
                CreateOneAsync().Forget();
                return null;
            }

            var view = _free.Pop();
            view.ResetDragState();
            view.transform.SetParent(parent, false);
            view.gameObject.SetActive(true);
            return view;
        }

        /// <summary>归还 View 到池中，隐藏并移回 poolContainer</summary>
        public void Return(CardViewController view)
        {
            if (view == null) return;
            view.ResetDragState();
            view.gameObject.SetActive(false);
            view.transform.SetParent(_poolContainer, false);
            _free.Push(view);
        }
    }
}
