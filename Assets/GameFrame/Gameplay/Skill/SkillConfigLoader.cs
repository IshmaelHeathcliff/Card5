using System.Collections.Generic;
using Data.Config;
using Gameplay.Skill.Effect;
using UnityEngine;


namespace Gameplay.Skill
{
    public class SkillCreationContext
    {
        public ISkill Skill { get; set; }
        public SkillCreateEnv Env { get; set; }
    }

    public class SkillConfigLoader
    {
        readonly SkillEffectConfigLoader _skillEffectConfigLoader = new();

        List<IEffect> CreateSkillEffects(IEnumerable<SkillEffectConfig> effectConfigs, SkillCreationContext context)
        {
            List<IEffect> skillEffects = new();

            if (effectConfigs == null)
            {
                return skillEffects;
            }

            foreach (SkillEffectConfig effectConfig in effectConfigs)
            {
                IEffect effect = _skillEffectConfigLoader.CreateEffect(effectConfig, context);
                if (effect != null)
                {
                    skillEffects.Add(effect);
                }
                else
                {
                    Debug.LogError($"创建技能效果失败: {effectConfig.GetType()}");
                }
            }
            return skillEffects;
        }

        public ISkill CreateSkill(SkillConfig skillConfig, SkillCreateEnv env)
        {
            var context = new SkillCreationContext { Env = env };

            List<IEffect> skillEffectsOnEnable = new();
            List<IEffect> skillEffectsOnUpdate = new();

            switch (skillConfig)
            {
                case AttackSkillConfig attackSkillConfig:
                    context.Skill = new AttackSkill(attackSkillConfig, env.Model.Stats);
                    skillEffectsOnUpdate.AddRange(CreateSkillEffects(attackSkillConfig.AttackEffectConfigs, context));
                    break;
                case RepetitiveSkillConfig repetitiveSkillConfig:
                    context.Skill = new RepetitiveSkill(repetitiveSkillConfig, env.Model.Stats);
                    break;
                case OneTimeSkillConfig oneTimeSkillConfig:
                    context.Skill = new OneTimeSkill(oneTimeSkillConfig, env.Model.Stats);
                    break;
                default:
                    Debug.LogError($"未知的技能类型: {skillConfig.GetType()}");
                    return null;
            }

            skillEffectsOnEnable.AddRange(CreateSkillEffects(skillConfig.SkillEffectConfigsOnEnable, context));
            skillEffectsOnUpdate.AddRange(CreateSkillEffects(skillConfig.SkillEffectConfigsOnUse, context));

            context.Skill.SetEffects(skillEffectsOnEnable, skillEffectsOnUpdate);

            return context.Skill;
        }
    }

    public class SkillEffectConfigLoader
    {
        public IEffect CreateEffect(SkillEffectConfig skillConfig, SkillCreationContext context)
        {
            IEffect effect = skillConfig switch
            {
                HealEffectConfig healEffectConfig => new HealEffect(healEffectConfig, context.Env.Model),
                AttackEffectConfig attackEffectConfig => new AttackEffect(attackEffectConfig, context.Env.Model),
                ModifierEffectConfig modifierEffectConfig => new ModifierEffect(modifierEffectConfig, context.Env.Model, context.Env.ModifierSystem),
                AcquireResourceEffectConfig acquireResourceEffectConfig => new AcquireResourceEffect(acquireResourceEffectConfig, context.Env.Model, context.Env.ResourceSystem),
                StatusWithTimeEffectConfig statusWithTimeEffectConfig => new StatusWithTimeEffect(statusWithTimeEffectConfig, context.Env.Model, context.Env.StatusCreateSystem),
                StatusEffectConfig statusEffectConfig => new StatusEffect(statusEffectConfig, context.Env.Model, context.Env.StatusCreateSystem),
                NestedEffectConfig nestedEffectConfig => CreateNestedEffect(nestedEffectConfig, context),
                AcquireSkillEffectConfig acquireSkillEffectConfig => new AcquireSkillEffect(acquireSkillEffectConfig, context.Env.Model, context.Env.SkillSystem),
                RemoveSkillEffectConfig removeSkillEffectConfig => new RemoveSkillEffect(removeSkillEffectConfig, context.Env.Model, context.Env.SkillSystem),
                _ => null,
            };

            if (effect != null)
            {
                effect.Owner = context.Skill;
            }

            return effect;
        }

        public IEffect CreateNestedEffect(NestedEffectConfig skillConfig, SkillCreationContext context)
        {
            if (skillConfig?.ChildEffects == null || context?.Env == null)
            {
                Debug.LogError("创建嵌套技能效果失败: skillConfig或context为null");
                return null;
            }

            List<IEffect> childEffects = new();
            foreach (SkillEffectConfig effectConfig in skillConfig.ChildEffects)
            {
                IEffect childEffect = CreateEffect(effectConfig, context);
                if (childEffect != null)
                {
                    childEffects.Add(childEffect);
                }
                else
                {
                    Debug.LogError($"创建子技能效果失败: {effectConfig.GetType()}, 描述: {effectConfig.Description ?? "无描述"}");
                }
            }

            IEffect effect = skillConfig switch
            {
                FixedRepeatEffectConfig fixedRepeatEffectConfig => new FixedRepeatEffect(fixedRepeatEffectConfig, context.Env.Model, childEffects),
                RollDiceEffectConfig rollDiceEffectConfig => new RollDiceEffect(rollDiceEffectConfig, context.Env.Model, childEffects, context.Env.CountSystem),
                RandomValueEffectConfig randomValueEffectConfig => new RandomValueEffect(randomValueEffectConfig, context.Env.Model, childEffects),
                CountIncrementEffectConfig countIncrementEffectConfig => new CountIncrementEffect(countIncrementEffectConfig, context.Env.Model, childEffects, context.Env.CountSystem),
                OnValueEffectConfig diceOnValueEffectConfig => new OnValueEffect(diceOnValueEffectConfig, context.Env.Model, childEffects),
                RandomChildEffectConfig randomChildEffectConfig => new RandomChildEffect(randomChildEffectConfig, context.Env.Model, childEffects),
                ConditionalSkillEffectConfig conditionalSkillEffectConfig => new ConditionalSkillEffect(conditionalSkillEffectConfig, context.Env.Model, childEffects),
                not null => new NestedSkillEffect<NestedEffectConfig>(skillConfig, context.Env.Model, childEffects),
            };

            effect.Owner = context.Skill;

            return effect;
        }
    }
}
