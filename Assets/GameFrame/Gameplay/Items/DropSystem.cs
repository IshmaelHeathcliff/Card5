using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.Config;
using Data.SaveLoad;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Data;

namespace Gameplay.Items
{
    public class DropSystem : AbstractSystem
    {
        Dictionary<string, DropConfig> _dropConfigs = new();
        readonly List<GameObject> _dropPrefabs = new();

        const string JsonPath = "Preset";
        const string JsonName = "Drops.json";

        readonly List<AsyncOperationHandle<GameObject>> _dropPrefabHandles = new();

        protected override void OnInit()
        {
            LoadDrops().Forget();
        }

        protected override void OnDeinit()
        {
            foreach (AsyncOperationHandle<GameObject> handle in _dropPrefabHandles)
            {
                AssetsManager.Release(handle);
            }
        }

        async UniTaskVoid LoadDrops()
        {
            var dropConfigs = this.GetUtility<SaveLoadUtility>().Load<List<DropConfig>>(JsonName, JsonPath);
            foreach (DropConfig dropConfig in dropConfigs)
            {
                AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(dropConfig.DropPrefab);
                _dropPrefabHandles.Add(handle);
                GameObject prefab = await handle;
                _dropPrefabs.Add(prefab);
                _dropConfigs.Add(dropConfig.DropID, dropConfig);
            }
        }

        public string GetDropAddress(string dropId)
        {
            if (_dropConfigs.TryGetValue(dropId, out DropConfig dropConfig))
            {
                return dropConfig.DropPrefab;
            }

            Debug.LogError($"Drop prefab not found: {dropId}");
            return null;
        }

    }
}
