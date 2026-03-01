using System.Collections.Generic;
using System.Linq;
using Data.Config;
using Gameplay.Character;
using UnityEngine;

namespace Gameplay.Skill.Effect
{
    public class RandomValueEffect : NestedSkillEffect<RandomValueEffectConfig>
    {
        public RandomValueEffect(RandomValueEffectConfig skillEffectConfig, ICharacterModel model, IEnumerable<IEffect> childEffects) : base(skillEffectConfig, model, childEffects)
        {
        }

        protected override void OnApply()
        {
            int value = Random.Range(SkillEffectConfig.Min, SkillEffectConfig.Max + 1);

            foreach (IEffect childEffect in ChildEffects)
            {
                if (childEffect is IEffect<int> effect)
                {
                    effect.Apply(value);
                }
                else
                {
                    childEffect.Apply();
                }
            }
        }
    }

    public class RollDiceEffect : RandomValueEffect
    {
        readonly CountSystem _countSystem;
        public RollDiceEffect(RollDiceEffectConfig skillEffectConfig, ICharacterModel model, IEnumerable<IEffect> childEffects, CountSystem countSystem) : base(skillEffectConfig, model, childEffects)
        {
            _countSystem = countSystem;
        }

        protected override void OnApply()
        {
            base.OnApply();
            _countSystem.IncrementCount("RollDiceTimes", Model, 1);
        }
    }

    public class RandomChildEffect : NestedSkillEffect<RandomChildEffectConfig>
    {
        public RandomChildEffect(RandomChildEffectConfig skillEffectConfig, ICharacterModel model, IEnumerable<IEffect> childEffects) : base(skillEffectConfig, model, childEffects)
        {
        }

        protected override void OnApply()
        {
            var enabledEffects = ChildEffects.Where(effect => effect.IsEnabled).ToList();
            if (enabledEffects.Count == 0)
            {
                return;
            }

            // 随机选择一个子效果
            int randomIndex = Random.Range(0, enabledEffects.Count);
            IEffect selectedEffect = enabledEffects[randomIndex];

            // 应用选中的效果
            selectedEffect.Apply();
        }
    }
}
