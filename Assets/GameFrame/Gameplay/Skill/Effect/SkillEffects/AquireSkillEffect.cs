using Data.Config;
using Gameplay.Character;

namespace Gameplay.Skill.Effect
{
    public class AcquireSkillEffect : SkillEffect<AcquireSkillEffectConfig>
    {
        readonly SkillSystem _skillSystem;

        public AcquireSkillEffect(AcquireSkillEffectConfig config, ICharacterModel model, SkillSystem skillSystem) : base(config, model)
        {
            _skillSystem = skillSystem;
            Description = $"获取技能 {config.SkillID}";
        }

        protected override void OnApply()
        {
            _skillSystem.AcquireSkill(SkillEffectConfig.SkillID, Model);
        }

        protected override void OnCancel()
        {
            // ? 是否应该取消技能获取
            // _skillSystem.RemoveSkill(SkillEffectConfig.SkillID, Model);
        }
    }
}
