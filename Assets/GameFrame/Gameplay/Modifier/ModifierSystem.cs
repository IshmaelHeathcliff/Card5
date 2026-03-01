using System.Collections.Generic;
using Data.Config;
using Data.SaveLoad;
using Gameplay.Stat;
using UnityEngine;

namespace Gameplay.Modifier
{
    public class ModifierSystem : AbstractSystem
    {
        Dictionary<string, ModifierConfig> _modifierConfigCache = new();
        const string JsonPath = "Preset";
        const string JsonName = "Modifiers.json";

        void Load()
        {
            _modifierConfigCache = new Dictionary<string, ModifierConfig>();
            List<ModifierConfig> modifierConfigList = this.GetUtility<SaveLoadUtility>().Load<List<ModifierConfig>>(JsonName, JsonPath);
            foreach (ModifierConfig modifierConfig in modifierConfigList)
            {
                _modifierConfigCache.Add(modifierConfig.ModifierID, modifierConfig);
            }
        }

        public ModifierConfig GetModifierConfig(string id)
        {
            if (_modifierConfigCache == null)
            {
                Load();
            }

            if (!_modifierConfigCache.TryGetValue(id, out ModifierConfig modifierConfig))
            {
                Debug.LogError($"modifierID {id} is not registered");
                return null;
            }

            return modifierConfig;
        }

        public T GetModifierConfig<T>(string id) where T : ModifierConfig
        {
            ModifierConfig modifierConfig = GetModifierConfig(id);
            if (modifierConfig == null)
            {
                return null;
            }

            if (modifierConfig is not T t)
            {
                Debug.LogError($"modifierID {id} is not a {typeof(T).Name}");
                return null;
            }

            return t;
        }

        readonly Dictionary<string, IModifierFactory> _modifierFactories = new();

        public void ClearModifierFactories()
        {
            _modifierFactories.Clear();
        }

        public void RegisterFactory(IModifierFactory factory)
        {
            _modifierFactories.Add(factory.FactoryID, factory);
        }

        public void UnregisterFactory(string factoryID)
        {
            _modifierFactories.Remove(factoryID);
        }

        public IStat GetStat(IStatModifier modifier)
        {
            var factory = GetModifierFactory<IStatModifierFactory>(modifier.FactoryID);
            if (factory == null)
            {
                return null;
            }

            return factory.GetStat(modifier.ModifierConfig);
        }

        public IModifierFactory GetModifierFactory(string factoryID)
        {
            if (!_modifierFactories.TryGetValue(factoryID, out IModifierFactory factory))
            {
                Debug.LogError($"factoryID {factoryID} is not registered");
                return null;
            }

            return factory;
        }

        public T GetModifierFactory<T>(string factoryID) where T : IModifierFactory
        {
            IModifierFactory factory = GetModifierFactory(factoryID);
            if (factory == null)
            {
                return default;
            }

            if (factory is not T t)
            {
                Debug.LogError($"factoryID {factoryID} is not a {typeof(T).Name}");
                return default;
            }

            return t;
        }


        public IStatModifier CreateStatModifier(string modifierId, string factoryID)
        {
            IStatModifierFactory factory = GetModifierFactory<IStatModifierFactory>(factoryID);
            if (factory == null)
            {
                return null;
            }

            return CreateStatModifier(modifierId, factory);
        }

        public IStatModifier CreateStatModifier(string modifierId, IStatModifierFactory factory)
        {
            StatModifierConfig modifierConfig = GetModifierConfig<StatModifierConfig>(modifierId);
            if (modifierConfig == null)
            {
                return null;
            }

            return factory.CreateModifier(modifierConfig);
        }

        public IStatModifier CreateStatModifier(string modifierId, string factoryID, int value)
        {

            IStatModifierFactory factory = GetModifierFactory<IStatModifierFactory>(factoryID);
            if (factory == null)
            {
                return null;
            }

            return CreateStatModifier(modifierId, factory, value);
        }

        public IStatModifier CreateStatModifier(string modifierId, IStatModifierFactory factory, int value)
        {
            StatModifierConfig modifierConfig = GetModifierConfig<StatModifierConfig>(modifierId);
            if (modifierConfig == null)
            {
                return null;
            }

            if (factory == null)
            {
                Debug.LogError($"factory is null, modifierId: {modifierId}");
                return null;
            }

            return factory.CreateModifier(modifierConfig, value);
        }

        protected override void OnInit()
        {
            Load();
        }
    }
}
