using Data.Config;
using Gameplay.Character;
using Gameplay.Stat;

namespace Gameplay.Skill.Effect
{
    public class HealEffect : SkillEffect<HealEffectConfig>
    {
        public HealEffect(HealEffectConfig config, ICharacterModel model) : base(config, model)
        {
        }

        protected override void OnApply()
        {
            var health = Model.Stats.GetStat("Health") as IConsumableStat;
            health.ChangeCurrentValue(SkillEffectConfig.Amount);
        }

        protected override void OnCancel()
        {
        }
    }
}
