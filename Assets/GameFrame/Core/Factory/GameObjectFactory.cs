using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

namespace Core.Factory
{
    public class SingleGameObjectFactory : IAsyncObjectFactory<GameObject>
    {
        readonly AssetReferenceGameObject _reference;

        public SingleGameObjectFactory(AssetReferenceGameObject reference)
        {
            _reference = reference;
        }

        public SingleGameObjectFactory(string address)
        {
            _reference = new AssetReferenceGameObject(address);
        }

        public virtual async UniTask<GameObject> Create()
        {
            GameObject obj = await Addressables.InstantiateAsync(_reference);
            obj.SetActive(false);
            return obj;
        }

        public virtual async UniTask<GameObject> Create(Transform parent)
        {
            GameObject obj = await Addressables.InstantiateAsync(_reference, parent);
            obj.SetActive(false);
            return obj;
        }

        public virtual void Destroy(GameObject obj)
        {
            Addressables.ReleaseInstance(obj);
        }
    }

    public class MultiGameObjectFactory : IAsyncObjectFactory<GameObject>
    {
        readonly Dictionary<string, AssetReferenceGameObject> _references = new();

        public MultiGameObjectFactory()
        {
        }

        public List<string> GetReferences()
        {
            return _references.Keys.ToList();
        }

        public void AddReference(string id, AssetReferenceGameObject reference)
        {
            _references.Add(id, reference);
        }

        public void AddReference(string id, string address)
        {
            _references.Add(id, new AssetReferenceGameObject(address));
        }

        public void RemoveReference(string id)
        {
            _references.Remove(id);
        }

        /// <summary>
        /// 返回第一个地址的实例
        /// </summary>
        /// <returns></returns>
        public virtual async UniTask<GameObject> Create()
        {
            if (_references.Count == 0)
            {
                Debug.LogError($"Empty GameObjectFactory");
                return null;
            }

            GameObject obj = await Addressables.InstantiateAsync(_references.Values.First());
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// 返回指定地址的实例
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public virtual async UniTask<GameObject> Create(string address, Transform parent)
        {
            if (_references.TryGetValue(address, out AssetReferenceGameObject reference))
            {
                GameObject obj = await Addressables.InstantiateAsync(reference, parent);
                obj.SetActive(false);
                return obj;
            }

            Debug.LogError($"GameObjectFactory: {address} not found");
            return null;
        }

        /// <summary>
        /// 销毁实例
        /// </summary>
        /// <param name="obj"></param>
        public virtual void Destroy(GameObject obj)
        {
            Addressables.ReleaseInstance(obj);
        }
    }
}
