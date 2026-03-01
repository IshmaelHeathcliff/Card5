using System.Collections.Generic;
using Data.Config;
using Gameplay.Character;

namespace Gameplay.Skill.Effect
{

    public class OnValueEffect : NestedSkillEffect<OnValueEffectConfig>, IEffect<int>
    {
        public int Value { get; set; }

        public OnValueEffect(OnValueEffectConfig skillEffectConfig, ICharacterModel model, IEnumerable<IEffect> childEffects) : base(skillEffectConfig, model, childEffects)
        {
            Value = skillEffectConfig.Value;
            Description = $"当数值为 {Value} 时触发";
        }

        public void Apply(int value)
        {
            if (value == Value)
            {
                base.OnApply();
            }
        }

        public void Cancel(int value)
        {
            if (value == Value)
            {
                base.OnCancel();
            }
        }
    }
}
