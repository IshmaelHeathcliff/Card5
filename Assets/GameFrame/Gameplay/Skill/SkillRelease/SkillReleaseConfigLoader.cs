using System;
using System.Linq;
using Data.Config;

namespace Gameplay.Skill
{
    public class SkillReleaseConfigLoader
    {
        readonly SkillReleaseConditionConfigLoader _skillReleaseConditionConfigLoader = new();
        readonly SkillReleaseRewardConfigLoader _skillReleaseRewardConfigLoader = new();

        public SkillReleaseRule CreateRule(SkillReleaseRuleConfig config)
        {
            return new SkillReleaseRule(config.ID, _skillReleaseConditionConfigLoader.CreateCondition(config.Condition), _skillReleaseRewardConfigLoader.CreateReward(config.Reward), config.IsOnce);
        }
    }

    public class SkillReleaseConditionConfigLoader
    {
        public ISkillReleaseCondition CreateCondition(SkillReleaseConditionConfig config)
        {
            switch (config)
            {
                case AnySkillsCountReleaseConditionConfig anySkillCountConfig:
                    return new AnySkillsCountReleaseCondition(anySkillCountConfig.RequiredSkillIDs, anySkillCountConfig.SkillsToRelease, anySkillCountConfig.RequiredCount, anySkillCountConfig.Description);
                case SpecificSkillsReleaseConditionConfig specificSkillsConfig:
                    return new SpecificSkillsReleaseCondition(specificSkillsConfig.RequiredSkillIDs, specificSkillsConfig.SkillsToRelease, specificSkillsConfig.Description);
                case ValueCountConditionConfig valueCountConfig:
                    return new ValueCountCondition(valueCountConfig.ValueID, valueCountConfig.Value, valueCountConfig.RequiredSkillIDs, valueCountConfig.SkillsToRelease, valueCountConfig.Description);
                case CompositeAndConditionConfig compositeAndConfig:
                    return new CompositeAndReleaseCondition(compositeAndConfig.Conditions.Select(c => CreateCondition(c)).ToList(), compositeAndConfig.Description);
                case CompositeOrConditionConfig compositeOrConfig:
                    return new CompositeOrReleaseCondition(compositeOrConfig.Conditions.Select(c => CreateCondition(c)).ToList(), compositeOrConfig.Description);
                case CompositeCountConditionConfig compositeCountConfig:
                    return new CompositeCountReleaseCondition(compositeCountConfig.Count, compositeCountConfig.Conditions.Select(c => CreateCondition(c)).ToList(), compositeCountConfig.Description);
                default:
                    throw new Exception($"Unknown skill release condition type: {config.GetType().Name}");
            }
        }
    }

    public class SkillReleaseRewardConfigLoader
    {
        public ISkillReleaseReward CreateReward(SkillReleaseRewardConfig config)
        {
            switch (config)
            {
                case SpecificSkillsReleaseRewardConfig specificSkillsConfig:
                    return new SpecificSkillsReleaseReward(specificSkillsConfig.NewSkillIDs, specificSkillsConfig.Description);
                default:
                    throw new Exception($"Unknown skill release reward type: {config.GetType().Name}");
            }
        }

    }
}
