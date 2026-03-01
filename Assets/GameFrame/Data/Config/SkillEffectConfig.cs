using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Config
{
    public abstract class SkillEffectConfig
    {
        [ShowInInspector][ReadOnly] public string Description { get; set; }
    }


    public class NestedEffectConfig : SkillEffectConfig
    {
        [ShowInInspector, PropertyOrder(int.MaxValue)] public List<SkillEffectConfig> ChildEffects { get; set; }
    }

    public class AttackEffectConfig : SkillEffectConfig
    {
        [ShowInInspector] public string AttackerID { get; set; }

        public AttackEffectConfig()
        {
            Description = "创建攻击器";
        }
    }

    public class AcquireSkillEffectConfig : SkillEffectConfig
    {
        [ShowInInspector] public string SkillID { get; set; }

        public AcquireSkillEffectConfig()
        {
            Description = "获取技能";
        }
    }

    public class RemoveSkillEffectConfig : SkillEffectConfig
    {
        [ShowInInspector] public string SkillID { get; set; }

        public RemoveSkillEffectConfig()
        {
            Description = "移除技能";
        }
    }

    public class ModifierEffectConfig : SkillEffectConfig
    {
        [ShowInInspector] public string ModifierID { get; set; }
        [ShowInInspector] public int Value { get; set; }

        public ModifierEffectConfig()
        {
            Description = "添加词条";
        }
    }

    public class LocalModifierEffectConfig : ModifierEffectConfig
    {
        [ShowInInspector] public string SkillID { get; set; }
        public LocalModifierEffectConfig()
        {
            Description = "添加局部词条";
        }
    }

    public class CountIncrementEffectConfig : NestedEffectConfig
    {
        [ShowInInspector] public string CountValueID { get; set; }
        [ShowInInspector] public int Increment { get; set; }

        public CountIncrementEffectConfig()
        {
            Description = "增量触发";
        }
    }

    public class RandomValueEffectConfig : NestedEffectConfig
    {
        [ShowInInspector] public int Min { get; set; }
        [ShowInInspector] public int Max { get; set; }

        public RandomValueEffectConfig()
        {
            Description = "范围内随机数值并触发";
        }
    }

    public class RollDiceEffectConfig : RandomValueEffectConfig
    {
        public RollDiceEffectConfig()
        {
            Description = "掷骰子";
            Min = 1;
            Max = 6;
        }
    }

    public class OnValueEffectConfig : NestedEffectConfig
    {
        [ShowInInspector] public int Value { get; set; }

        public OnValueEffectConfig()
        {
            Description = "对应数值时触发";
        }
    }

    public class AcquireResourceEffectConfig : SkillEffectConfig
    {
        [ShowInInspector] public string ResourceID { get; set; }
        [ShowInInspector] public int Amount { get; set; }

        public AcquireResourceEffectConfig()
        {
            Description = "获取资源";
        }
    }

    public class StatusEffectConfig : SkillEffectConfig
    {
        [ShowInInspector] public string StatusID { get; set; }
        [ShowInInspector] public List<int> Values { get; set; } = null;

        public StatusEffectConfig()
        {
            Description = "状态效果";
        }
    }

    public class StatusWithTimeEffectConfig : StatusEffectConfig
    {
        [ShowInInspector] public int Duration { get; set; } = -1;

        public StatusWithTimeEffectConfig()
        {
            Description = "状态效果(持续时间)";
        }
    }

    public class FixedRepeatEffectConfig : NestedEffectConfig
    {
        [ShowInInspector] public int Interval { get; set; }

        public FixedRepeatEffectConfig()
        {
            Description = "固定时间触发";
        }
    }

    public class HealEffectConfig : SkillEffectConfig
    {
        [ShowInInspector] public int Amount { get; set; }

        public HealEffectConfig()
        {
            Description = "治疗";
        }
    }

    public class RandomChildEffectConfig : NestedEffectConfig
    {
        public RandomChildEffectConfig()
        {
            Description = "随机启用一个子效果";
        }
    }

    public class ConditionalSkillEffectConfig : NestedEffectConfig
    {
        [ShowInInspector] public List<string> RequiredSkillIDs { get; set; }

        public ConditionalSkillEffectConfig()
        {
            Description = "拥有指定技能时生效";
        }
    }
}
