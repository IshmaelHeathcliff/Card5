using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Data.Config;
using Data.SaveLoad;
using Gameplay.Skill;

namespace Gameplay.Damage.Attackers
{
    public class AttackerSystem : AbstractSystem
    {
        const string JsonPath = "Preset";
        const string JsonName = "Attackers.json";

        readonly Dictionary<string, AttackerConfig> _attackerConfigs = new();

        void Load()
        {
            _attackerConfigs.Clear();
            List<AttackerConfig> configs = this.GetUtility<SaveLoadUtility>().Load<List<AttackerConfig>>(JsonName, JsonPath);
            foreach (AttackerConfig config in configs)
            {
                _attackerConfigs.Add(config.AttackerID, config);
            }
        }

        public AttackerConfig GetAttackerConfig(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("获取Attacker配置失败：ID为空");
                return null;
            }

            if (_attackerConfigs.TryGetValue(id, out AttackerConfig config))
            {
                return config;
            }

            Debug.LogWarning($"未找到ID为{id}的Attacker配置");
            return null;
        }

        protected override void OnInit()
        {
            // 系统初始化逻辑
            Load();
        }
    }
}
