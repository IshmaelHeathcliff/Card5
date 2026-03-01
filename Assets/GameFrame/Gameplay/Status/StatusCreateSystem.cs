using System.Collections.Generic;
using System.Linq;
using Data.Config;
using Data.SaveLoad;
using Gameplay.Modifier;
using UnityEngine;

namespace Gameplay.Status
{
    public class StatusCreateSystem : AbstractSystem
    {
        Dictionary<string, StatusConfig> _statusConfigCache;
        const string JsonPath = "Preset";
        const string JsonName = "Status.json";

        void Load()
        {
            _statusConfigCache = new Dictionary<string, StatusConfig>();
            List<StatusConfig> statusConfigList = this.GetUtility<SaveLoadUtility>().Load<List<StatusConfig>>(JsonName, JsonPath);
            foreach (StatusConfig statusConfig in statusConfigList)
            {
                _statusConfigCache.Add(statusConfig.ID, statusConfig);
            }
        }

        public StatusConfig GetStatusConfig(string id)
        {
            if (_statusConfigCache == null)
            {
                Load();
            }

            if (_statusConfigCache.TryGetValue(id, out StatusConfig config))
            {
                return config;
            }

            Debug.LogError($"StatusConfig not found: {id}");
            return null;
        }

        public IStatus CreateStatus(string id, string factoryID, List<int> values = null)
        {
            IStatModifierFactory factory = this.GetSystem<ModifierSystem>().GetModifierFactory<IStatModifierFactory>(factoryID);
            return CreateStatus(id, factory, values);
        }

        public IStatusWithTime CreateStatus(string id, string factoryID, int time = -1, List<int> values = null)
        {
            IStatModifierFactory factory = this.GetSystem<ModifierSystem>().GetModifierFactory<IStatModifierFactory>(factoryID);
            return CreateStatus(id, factory, time, values);
        }


        public IStatus CreateStatus(string id, IStatModifierFactory factory, List<int> values = null)
        {
            StatusConfig statusConfig = GetStatusConfig(id);
            ModifierSystem modifierSystem = this.GetSystem<ModifierSystem>();
            if (values != null && statusConfig.ModifierEntries.Count != values.Count)
            {
                Debug.LogError("values.Length != statusConfig.EntryID.Count");
                return null;
            }

            IEnumerable<IStatModifier> entries = statusConfig.ModifierEntries.Select(
                    (entry, i) => modifierSystem.CreateStatModifier(entry.ModifierID, factory, values != null ? values[i] : entry.Value));

            return new Status(statusConfig, entries);
        }

        public IStatusWithTime CreateStatus(string id, IStatModifierFactory factory, int time = -1, List<int> values = null)
        {
            StatusConfig statusConfig = GetStatusConfig(id);

            ModifierSystem modifierSystem = this.GetSystem<ModifierSystem>();

            if (values != null && statusConfig.ModifierEntries.Count != values.Count)
            {
                Debug.LogError("values.Length != statusConfig.EntryID.Count");
                return null;
            }

            var entries = new List<IModifier>();

            for (int i = 0; i < statusConfig.ModifierEntries.Count; i++)
            {
                entries.Add(modifierSystem.CreateStatModifier(statusConfig.ModifierEntries[i].ModifierID, factory, values != null ? values[i] : statusConfig.ModifierEntries[i].Value));
            }

            return new StatusWithTime(statusConfig, entries, time == -1 ? statusConfig.Duration : time);
        }



        protected override void OnInit()
        {
            _statusConfigCache = new Dictionary<string, StatusConfig>();
            Load();
        }
    }
}
