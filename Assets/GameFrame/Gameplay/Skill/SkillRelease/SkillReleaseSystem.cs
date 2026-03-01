using System.Collections.Generic;
using Data.Config;
using Data.SaveLoad;
using Gameplay.Character;

namespace Gameplay.Skill
{
    public class SkillReleaseSystem : AbstractSystem
    {
        readonly Dictionary<string, SkillReleaseRule> _releaseRules = new();
        readonly SkillReleaseConfigLoader _skillReleaseConfigLoader = new();

        const string JsonPath = "Preset";
        const string JsonName = "SkillReleaseRules.json";

        SkillSystem _skillSystem;

        // 保存注册的事件，用于在OnDeinit时取消注册
        readonly List<IUnRegister> _unRegisters = new();

        protected override void OnInit()
        {
            _skillSystem = this.GetSystem<SkillSystem>();

            LoadRules();
        }

        void LoadRules()
        {
            List<SkillReleaseRuleConfig> configs = this.GetUtility<SaveLoadUtility>().Load<List<SkillReleaseRuleConfig>>(JsonName, JsonPath);

            if (configs != null)
            {
                foreach (SkillReleaseRuleConfig config in configs)
                {
                    _releaseRules.Add(config.ID, _skillReleaseConfigLoader.CreateRule(config));
                }
            }
        }

        public void RegisterConditions(ICharacterModel model)
        {
            foreach (SkillReleaseRule rule in _releaseRules.Values)
            {
                _unRegisters.Add(this.RegisterEvent<SkillAcquiredEvent>(rule.Condition.CheckCondition));

                switch (rule.Condition)
                {
                    case ValueCountCondition valueCountCondition:
                        _unRegisters.Add(this.GetSystem<CountSystem>().Register(valueCountCondition.ValueID, model, e => valueCountCondition.CheckCondition(e)));
                        break;
                }
            }
        }

        public void RegisterRelease(ICharacterModel model)
        {
            foreach (SkillReleaseRule rule in _releaseRules.Values)
            {
                RegisterSkillRelease(rule);
            }
        }

        void RegisterSkillRelease(SkillReleaseRule rule)
        {
            _unRegisters.Add(rule.Condition.OnRelease.Register(e =>
            {
                if (!CanTriggerRule(rule)) return;

                foreach (string skillID in new List<string>(rule.Condition.SkillsToRelease))
                {
                    _skillSystem.ReleaseSkill(skillID, e.Model);
                }

                MarkRuleAsTriggered(rule);

                if (rule.Reward is not SpecificSkillsReleaseReward skillReleaseReward)
                    return;

                foreach (string skillID in skillReleaseReward.NewSkillIDs)
                {
                    _skillSystem.AcquireSkill(skillID, e.Model);
                }
            }));
        }

        bool CanTriggerRule(SkillReleaseRule rule)
        {
            return !(rule.IsOneTime && rule.HasTriggered);
        }

        void MarkRuleAsTriggered(SkillReleaseRule rule)
        {
            rule.HasTriggered = true;
        }

        protected override void OnDeinit()
        {
            foreach (IUnRegister unRegister in _unRegisters)
            {
                unRegister.UnRegister();
            }

            _unRegisters.Clear();
        }
    }
}
