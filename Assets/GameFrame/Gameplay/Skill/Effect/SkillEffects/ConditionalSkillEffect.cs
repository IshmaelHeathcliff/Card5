using System.Collections.Generic;
using Data.Config;
using Gameplay.Character;

namespace Gameplay.Skill.Effect
{
    public class ConditionalSkillEffect : NestedSkillEffect<ConditionalSkillEffectConfig>
    {
        public ConditionalSkillEffect(ConditionalSkillEffectConfig skillEffectConfig, ICharacterModel model, IEnumerable<IEffect> childEffects) : base(skillEffectConfig, model, childEffects)
        {
            if (skillEffectConfig.RequiredSkillIDs != null && skillEffectConfig.RequiredSkillIDs.Count > 0)
            {
                Description = $"拥有技能 {string.Join(", ", skillEffectConfig.RequiredSkillIDs)} 时生效";
            }
        }

        protected override bool OnEnable()
        {
            // 先启用所有子效果
            bool result = base.OnEnable();

            // 检查角色是否拥有所需技能
            bool hasRequiredSkills = Model.HasSkills(SkillEffectConfig.RequiredSkillIDs);

            return result && hasRequiredSkills;
        }
    }
}
