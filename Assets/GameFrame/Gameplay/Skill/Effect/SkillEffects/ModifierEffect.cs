using System.Collections.Generic;
using Data.Config;
using Gameplay.Character;
using Gameplay.Modifier;

namespace Gameplay.Skill.Effect
{
    public class ModifierEffect : SkillEffect<ModifierEffectConfig>
    {
        readonly List<IStatModifier> _modifiers = new();
        readonly ModifierSystem _modifierSystem;

        public ModifierEffect(ModifierEffectConfig config, ICharacterModel model, ModifierSystem modifierSystem) : base(config, model)
        {
            _modifierSystem = modifierSystem;
        }


        protected override void OnApply()
        {
            IStatModifierFactory factory;
            if (SkillEffectConfig is LocalModifierEffectConfig localConfig)
            {
                factory = Model.GetSkill(localConfig.SkillID)?.SkillStats;
            }
            else
            {
                factory = Model.Stats;
            }

            IStatModifier modifier = _modifierSystem.CreateStatModifier(SkillEffectConfig.ModifierID, factory, SkillEffectConfig.Value);
            if (modifier != null)
            {
                _modifiers.Add(modifier);
                modifier.Register();
            }
        }


        protected override void OnCancel()
        {
            foreach (IStatModifier modifier in _modifiers)
            {
                modifier.Unregister();
            }

            _modifiers.Clear();
        }
    }
}
