using Data.Config;
using Gameplay.Character;
using Gameplay.Items;
using UnityEngine;

namespace Gameplay.Skill.Effect
{
    public class AcquireResourceEffect : SkillEffect<AcquireResourceEffectConfig>, IEffect<int>
    {
        readonly ResourceSystem _resourceSystem;
        int _acquireValue;

        public AcquireResourceEffect(AcquireResourceEffectConfig config, ICharacterModel model, ResourceSystem resourceSystem) : base(config, model)
        {
            _resourceSystem = resourceSystem;
            Description = $"获取资源 {config.ResourceID} {config.Amount}";
        }

        bool CheckModel(ICharacterModel model, out IHasResources resourceModel)
        {
            if (model is IHasResources resources)
            {
                resourceModel = resources;
                return true;
            }

            Debug.LogError("AcquireResourceEffect: model is not IHasResources");
            resourceModel = null;
            return false;
        }

        protected override void OnApply()
        {
            _acquireValue += SkillEffectConfig.Amount;

            if (CheckModel(Model, out IHasResources model))
            {
                _resourceSystem.AcquireResource(SkillEffectConfig.ResourceID, SkillEffectConfig.Amount, model);
            }
        }

        protected override void OnCancel()
        {
            if (CheckModel(Model, out IHasResources model))
            {
                _resourceSystem.AcquireResource(SkillEffectConfig.ResourceID, -_acquireValue, model);
            }
        }

        public void Apply(int value)
        {
            _acquireValue += value;
            if (CheckModel(Model, out IHasResources model))
            {
                _resourceSystem.AcquireResource(SkillEffectConfig.ResourceID, value, model);
            }
        }

        public void Cancel(int value)
        {
            if (CheckModel(Model, out IHasResources model))
            {
                _resourceSystem.AcquireResource(SkillEffectConfig.ResourceID, -_acquireValue, model);
            }
        }
    }
}
