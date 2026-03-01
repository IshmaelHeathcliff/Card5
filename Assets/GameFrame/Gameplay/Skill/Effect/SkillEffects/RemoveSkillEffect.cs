using Data.Config;
using Gameplay.Character;

namespace Gameplay.Skill.Effect
{
    public class RemoveSkillEffect : SkillEffect<RemoveSkillEffectConfig>
    {
        readonly SkillSystem _skillSystem;

        public RemoveSkillEffect(RemoveSkillEffectConfig config, ICharacterModel model, SkillSystem skillSystem) : base(config, model)
        {
            _skillSystem = skillSystem;
            Description = $"移除技能 {config.SkillID}";
        }

        protected override void OnApply()
        {
            _skillSystem.RemoveSkill(SkillEffectConfig.SkillID, Model);
        }

        protected override void OnCancel()
        {
            _skillSystem.AcquireSkill(SkillEffectConfig.SkillID, Model);
        }
    }
}
