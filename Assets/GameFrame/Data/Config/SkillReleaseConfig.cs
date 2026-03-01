using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Config
{
    public class SkillReleaseRuleConfig
    {
        [ShowInInspector] public string ID { get; set; }
        [ShowInInspector] public string Name { get; set; }
        [ShowInInspector] public string Description { get; set; }
        [ShowInInspector] public bool IsOnce { get; set; } = true;
        [ShowInInspector] public SkillReleaseConditionConfig Condition { get; set; }
        [ShowInInspector] public SkillReleaseRewardConfig Reward { get; set; }
    }

    #region Reward
    public class SpecificSkillsReleaseRewardConfig : SkillReleaseRewardConfig
    {
        [ShowInInspector] public List<string> NewSkillIDs { get; set; } = new();
    }

    #endregion


    #region Condition

    public abstract class SkillReleaseConditionConfig
    {
        [ShowInInspector] public string Description { get; set; }
        [ShowInInspector] public List<string> SkillsToRelease { get; set; } = new();
        [ShowInInspector] public List<string> RequiredSkillIDs { get; set; } = new();

    }

    public abstract class SkillReleaseRewardConfig
    {
        [ShowInInspector] public string Description { get; set; }
    }


    public class SpecificSkillsReleaseConditionConfig : SkillReleaseConditionConfig
    {
    }

    public class AnySkillsCountReleaseConditionConfig : SkillReleaseConditionConfig
    {
        [ShowInInspector] public int RequiredCount { get; set; }
    }


    public class ValueCountConditionConfig : SkillReleaseConditionConfig
    {
        [ShowInInspector] public string ValueID { get; set; }
        [ShowInInspector] public int Value { get; set; }
    }

    public abstract class CompositeConditionConfig : SkillReleaseConditionConfig
    {
        [ShowInInspector] public List<SkillReleaseConditionConfig> Conditions { get; set; } = new();
    }

    public class CompositeAndConditionConfig : CompositeConditionConfig
    {
    }

    public class CompositeOrConditionConfig : CompositeConditionConfig
    {
    }

    public class CompositeCountConditionConfig : CompositeConditionConfig
    {
        [ShowInInspector] public int Count { get; set; }
    }

    #endregion
}
