using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gameplay.Items
{
    public enum ResourceType
    {
        Coin,
        Wood,
    }

    public interface IResourceContainer
    {
        int GetResourceCount(string id);
        void SetResourceCount(string id, int value);
        void AddResourceCount(string id, int value);

        int GetResourceCount(ResourceType type);
        void SetResourceCount(ResourceType type, int value);
        void AddResourceCount(ResourceType type, int value);

        IUnRegister Register(string id, Action<int> onValueChanged);
        IUnRegister RegisterWithInitValue(string id, Action<int> onValueChanged);
        void UnRegister(string id, Action<int> onValueChanged);
        IUnRegister Register(ResourceType type, Action<int> onValueChanged);
        IUnRegister RegisterWithInitValue(ResourceType type, Action<int> onValueChanged);
        void UnRegister(ResourceType type, Action<int> onValueChanged);
        IUnRegister RegisterCoin(Action<int> onValueChanged);
        IUnRegister RegisterWood(Action<int> onValueChanged);
        void UnRegisterCoin(Action<int> onValueChanged);
        void UnRegisterWood(Action<int> onValueChanged);
    }

    public class ResourceContainer : IResourceContainer
    {
        Dictionary<string, BindableProperty<int>> Resources { get; } = new();

        public ResourceContainer()
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                Resources.Add(type.ToString(), new BindableProperty<int>());
            }
        }

        public int GetResourceCount(ResourceType type)
        {
            return Resources[type.ToString()].Value;
        }

        public void SetResourceCount(ResourceType type, int value)
        {
            Resources[type.ToString()].Value = value;
        }

        public void AddResourceCount(ResourceType type, int value)
        {
            Resources[type.ToString()].Value += value;
        }

        public int GetResourceCount(string id)
        {
            if (Resources.TryGetValue(id, out BindableProperty<int> resource))
            {
                return resource.Value;
            }

            Debug.LogError($"Resource type not found: {id}");
            return 0;
        }

        public void SetResourceCount(string id, int value)
        {
            if (Resources.TryGetValue(id, out BindableProperty<int> resource))
            {
                resource.Value = value;
            }
            else
            {
                Debug.LogError($"Resource type not found: {id}");
            }

        }

        public void AddResourceCount(string id, int value)
        {
            if (Resources.TryGetValue(id, out BindableProperty<int> resource))
            {
                resource.Value += value;
            }
            else
            {
                Debug.LogError($"Resource type not found: {id}");
            }
        }

        public IUnRegister Register(string id, Action<int> onValueChanged)
        {
            return Resources[id].Register(onValueChanged);
        }

        public IUnRegister RegisterWithInitValue(string id, Action<int> onValueChanged)
        {
            return Resources[id].RegisterWithInitValue(onValueChanged);
        }

        public void UnRegister(string id, Action<int> onValueChanged)
        {
            Resources[id].UnRegister(onValueChanged);
        }

        public IUnRegister Register(ResourceType type, Action<int> onValueChanged)
        {
            return Resources[type.ToString()].Register(onValueChanged);
        }

        public IUnRegister RegisterWithInitValue(ResourceType type, Action<int> onValueChanged)
        {
            return Resources[type.ToString()].RegisterWithInitValue(onValueChanged);
        }

        public void UnRegister(ResourceType type, Action<int> onValueChanged)
        {
            Resources[type.ToString()].UnRegister(onValueChanged);
        }

        public IUnRegister RegisterCoin(Action<int> onValueChanged)
        {
            return Register(ResourceType.Coin, onValueChanged);
        }

        public IUnRegister RegisterWood(Action<int> onValueChanged)
        {
            return Register(ResourceType.Wood, onValueChanged);
        }

        public void UnRegisterCoin(Action<int> onValueChanged)
        {
            UnRegister(ResourceType.Coin, onValueChanged);
        }

        public void UnRegisterWood(Action<int> onValueChanged)
        {
            UnRegister(ResourceType.Wood, onValueChanged); ;
        }
    }
}
