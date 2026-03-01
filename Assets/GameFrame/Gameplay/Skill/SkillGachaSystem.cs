using System.Collections.Generic;
using System.Linq;
using Data.Config;
using Data.SaveLoad;
using Gameplay.Character;
using Gameplay.Character.Player;
using Gameplay.Items;
using Random = UnityEngine.Random;

namespace Gameplay.Skill
{


    public class SkillGachaSystem : AbstractSystem
    {
        const string PresetPath = "Preset";
        const string AddRulesPath = "SkillPoolAddRules.json";
        const string RemoveRulesPath = "SkillPoolRemoveRules.json";
        SkillSystem _skillSystem;
        ResourceSystem _resourceSystem;
        readonly List<SkillPoolAddRule> _skillAddRules = new();
        readonly List<SkillPoolRemoveRule> _skillRemoveRules = new();
        readonly SkillPoolAddRuleLoader _skillPoolAddRuleLoader = new();
        readonly SkillPoolRemoveRuleLoader _skillPoolRemoveRuleLoader = new();

        readonly List<IUnRegister> _unRegisters = new();

        readonly Dictionary<SkillRarity, int> _skillGachaWeights = new()
        {
            { SkillRarity.Common, 50 },
            { SkillRarity.Magic, 30 },
            { SkillRarity.Rare, 15 },
            { SkillRarity.Epic, 4 },
            { SkillRarity.Legendary, 1},
        };

        void Load()
        {
            _skillAddRules.Clear();
            List<SkillPoolAddRuleConfig> addRuleConfigs = this.GetUtility<SaveLoadUtility>().Load<List<SkillPoolAddRuleConfig>>(AddRulesPath, PresetPath);
            foreach (SkillPoolAddRuleConfig config in addRuleConfigs)
            {
                _skillAddRules.Add(_skillPoolAddRuleLoader.Load(config));
            }

            _skillRemoveRules.Clear();
            List<SkillPoolRemoveRuleConfig> removeRuleConfigs = this.GetUtility<SaveLoadUtility>().Load<List<SkillPoolRemoveRuleConfig>>(RemoveRulesPath, PresetPath);
            foreach (SkillPoolRemoveRuleConfig config in removeRuleConfigs)
            {
                _skillRemoveRules.Add(_skillPoolRemoveRuleLoader.Load(config));
            }
            // foreach (SkillPoolAddRuleConfig config in addRuleConfigs)
            // {
            //     Debug.Log($"卡池规则 {config.Name} 已添加");
            // }
        }

        public void InitSkillPool(ICharacterModel model, string path)
        {
            List<SkillInPool> initSkills = this.GetUtility<SaveLoadUtility>().Load<List<SkillInPool>>(path, PresetPath);
            foreach (SkillInPool skill in initSkills)
            {
                SkillConfig config = _skillSystem.GetSkillConfig(skill.Name);
                if (config != null)
                {
                    model.SkillPool.AddSkill(config);
                }
            }
        }

        SkillConfig GachaSkill(ICharacterModel model)
        {
            // 计算总权重和有效权重
            int totalWeight = 0;
            Dictionary<SkillRarity, int> effectiveWeights = new();
            foreach ((SkillRarity rarity, int weight) in _skillGachaWeights)
            {
                if (model.SkillPool.GetCount(rarity) > 0)
                {
                    totalWeight += weight;
                    effectiveWeights[rarity] = weight;
                }
            }

            // 如果没有可抽取的技能，返回null
            if (totalWeight == 0)
            {
                return null;
            }

            // 按权重随机选择稀有度
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach ((SkillRarity rarity, int weight) in effectiveWeights)
            {
                currentWeight += weight;
                if (randomValue < currentWeight)
                {
                    return model.SkillPool.PopRandomSkill(rarity);
                }
            }

            return null;
        }

        public List<SkillConfig> GachaSkills(ICharacterModel model, int count = 1)
        {

            if (model.SkillPool.GetCount() < count)
            {
                count = model.SkillPool.GetCount();
            }

            List<SkillConfig> result = new();

            if (count == 0)
            {
                return result;
            }

            if (model is not IHasResources resourceModel || _resourceSystem.GetResourceCount(ResourceType.Wood, resourceModel) < 1)
            {
                return result;
            }

            _resourceSystem.ConsumeResource(ResourceType.Wood, 1, resourceModel);

            for (int i = 0; i < count; i++)
            {
                SkillConfig config = GachaSkill(model);
                while (config == null && model.SkillPool.GetCount() > 0)
                {
                    config = GachaSkill(model);
                }

                if (config != null)
                {
                    result.Add(config);
                }
            }

            this.SendEvent(new GachaSkillsEvent(result, model));

            return result;
        }

        public void SelectSkill(ICharacterModel model, List<SkillConfig> configs, int index)
        {
            if (index < 0 || index >= configs.Count)
            {
                foreach (SkillConfig config in configs)
                {
                    model.SkillPool.AddSkill(config);
                }

                return;
            }

            for (int i = 0; i < configs.Count; i++)
            {
                if (i != index)
                {
                    model.SkillPool.AddSkill(configs[i]);
                }
            }

            _skillSystem.AcquireSkill(configs[index].ID, model);

            this.SendEvent(new SelectSkillEvent(configs, index, model));
        }

        public void CancelSelect(ICharacterModel model, List<SkillConfig> configs)
        {
            foreach (SkillConfig config in configs)
            {
                model.SkillPool.AddSkill(config);
            }

            this.SendEvent(new SelectSkillEvent(configs, -1, model));
        }


        void RegisterRules()
        {
            _unRegisters.Add(this.RegisterEvent<SkillAcquiredEvent>(OnSkillAcquired));
            _unRegisters.Add(this.RegisterEvent<SkillRemovedEvent>(OnSkillRemoved));

            foreach (SkillPoolAddRule rule in _skillAddRules)
            {
                if (rule is SpecificSkillsPoolAddRule specificAddRule)
                {
                    RegisterSpecificSkillsPoolAddRule(specificAddRule);
                }
            }

            foreach (SkillPoolRemoveRule rule in _skillRemoveRules)
            {
                if (rule is SpecificSkillsPoolRemoveRule specificRemoveRule)
                {
                    RegisterSpecificSkillsPoolRemoveRule(specificRemoveRule);
                }
            }
        }

        void RegisterSpecificSkillsPoolAddRule(SpecificSkillsPoolAddRule rule)
        {
            _unRegisters.Add(this.RegisterEvent<SkillAcquiredEvent>(e =>
            {
                if (!rule.RequiredSkillIDs.Contains(e.Skill.ID))
                {
                    return;
                }

                if (e.Model.HasSkills(rule.RequiredSkillIDs))
                {
                    IEnumerable<SkillConfig> skillConfigsToAdd = _skillSystem.GetSkillConfigs(rule.SkillIDsToAdd);
                    e.Model.SkillPool.AddSkills(skillConfigsToAdd);
                    // foreach (SkillConfig config in skillConfigsToAdd)
                    // {
                    //     Debug.Log($"Skill {config.ID} added to pool");
                    // }
                }
            })
            );
            // Debug.Log($"卡池规则 {rule.Name} 已注册");

        }

        void RegisterSpecificSkillsPoolRemoveRule(SpecificSkillsPoolRemoveRule rule)
        {
            _unRegisters.Add(this.RegisterEvent<SkillAcquiredEvent>(e =>
            {
                if (!rule.RequiredSkillIDs.Contains(e.Skill.ID))
                {
                    return;
                }

                if (e.Model.HasSkills(rule.RequiredSkillIDs))
                {
                    IEnumerable<SkillConfig> skillConfigsToRemove = _skillSystem.GetSkillConfigs(rule.SkillIDsToRemove);
                    e.Model.SkillPool.RemoveSkills(skillConfigsToRemove);
                }
            }));
        }

        void OnSkillAcquired(SkillAcquiredEvent e)
        {
            SkillConfig skillConfig = _skillSystem.GetSkillConfig(e.Skill.ID);
            e.Model.SkillPool.RemoveSkill(skillConfig);
        }

        void OnSkillRemoved(SkillRemovedEvent e)
        {
            // If this skill is a component in any remove rule, assume it's consumed and should not be added back to the pool.
            // if (_skillRemoveRules.Any(rule => rule.SkillIDsToRemove.Contains(e.Skill.ID)))
            // {
            //     return;
            // }

            // SkillConfig skillConfig = _skillSystem.GetSkillConfig(e.Skill.ID);
            // if (skillConfig != null)
            // {
            //     e.Model.SkillPool.AddSkill(skillConfig);
            // }
        }

        protected override void OnInit()
        {
            _skillSystem = this.GetSystem<SkillSystem>();
            _resourceSystem = this.GetSystem<ResourceSystem>();
            Load();
            RegisterRules();
        }

        protected override void OnDeinit()
        {
            base.OnDeinit();
            foreach (IUnRegister unRegister in _unRegisters)
            {
                unRegister.UnRegister();
            }
        }
    }

    public class SkillPoolAddRuleLoader
    {
        public SkillPoolAddRule Load(SkillPoolAddRuleConfig config)
        {
            return config switch
            {
                SpecificSkillsPoolAddRuleConfig specificConfig => new SpecificSkillsPoolAddRule(specificConfig),
                _ => null
            };
        }
    }

    public class SkillPoolRemoveRuleLoader
    {
        public SkillPoolRemoveRule Load(SkillPoolRemoveRuleConfig config)
        {
            return config switch
            {
                SpecificSkillsPoolRemoveRuleConfig specificConfig => new SpecificSkillsPoolRemoveRule(specificConfig),
                _ => null
            };
        }
    }
}

