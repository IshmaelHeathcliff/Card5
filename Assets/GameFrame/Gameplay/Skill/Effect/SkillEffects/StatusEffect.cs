using System.Collections.Generic;
using Data.Config;
using Gameplay.Character;
using Gameplay.Status;

namespace Gameplay.Skill.Effect
{
    public class StatusEffect : SkillEffect<StatusEffectConfig>
    {
        readonly StatusCreateSystem _statusCreateSystem;
        IStatus _status;
        public StatusEffect(StatusEffectConfig skillEffectConfig, ICharacterModel model, StatusCreateSystem statusCreateSystem) : base(skillEffectConfig, model)
        {
            _statusCreateSystem = statusCreateSystem;
        }

        protected override void OnApply()
        {
            _status = _statusCreateSystem.CreateStatus(SkillEffectConfig.StatusID, Model.Stats, SkillEffectConfig.Values);
            Model.StatusContainer.AddStatus(_status);
        }

        protected override void OnCancel()
        {
            Model.StatusContainer.RemoveStatus(_status);
        }
    }

    public class StatusWithTimeEffect : SkillEffect<StatusWithTimeEffectConfig>
    {
        readonly StatusCreateSystem _statusCreateSystem;
        IStatusWithTime _status;
        public StatusWithTimeEffect(StatusWithTimeEffectConfig skillEffectConfig, ICharacterModel model, StatusCreateSystem statusCreateSystem) : base(skillEffectConfig, model)
        {
            _statusCreateSystem = statusCreateSystem;
        }

        protected override void OnApply()
        {
            _status = _statusCreateSystem.CreateStatus(SkillEffectConfig.StatusID, Model.Stats, SkillEffectConfig.Duration, SkillEffectConfig.Values);
            Model.StatusContainer.AddStatus(_status);
        }

        protected override void OnCancel()
        {
            // Model.StatusContainer.RemoveStatus(_status);
        }

    }
}
