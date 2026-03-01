using System.Collections.Generic;
using System.Linq;
using Data.Config;
using Gameplay.Skill.Effect;
using Gameplay.Stat;

namespace Gameplay.Skill
{
    public enum SkillRarity
    {
        Common,
        Magic,
        Rare,
        Epic,
        Legendary,
    }
    public enum SkillType
    {
        OneTime,
        Repetitive,
        Attack
    }

    public interface ISkill
    {
        string ID { get; }
        string Name { get; }
        string Description { get; }
        string IconAddress { get; }
        bool ReleaseOnAcquire { get; }
        List<string> Keywords { get; }
        SkillRarity Rarity { get; }
        SkillStats SkillStats { get; }
        bool IsEnabled { get; }
        void Enable();
        void Disable();
        void Use();
        void Cancel();
        void SetEffects(IEnumerable<IEffect> effectsOnEnable, IEnumerable<IEffect> effectsOnUpdate);
    }

    public interface ISkill<T> : ISkill where T : SkillConfig
    {
        T SkillConfig { get; set; }
    }

    public abstract class Skill<T> : ISkill<T> where T : SkillConfig
    {
        public T SkillConfig { get; set; }
        public string ID => SkillConfig.ID;
        public string Name => SkillConfig.Name;
        public virtual string Description => SkillConfig.Description;
        public string IconAddress => SkillConfig.IconAddress;
        public bool ReleaseOnAcquire => SkillConfig.ReleaseOnAcquire;
        public virtual List<string> Keywords => SkillConfig.Keywords;
        public SkillRarity Rarity => SkillConfig.Rarity;
        public SkillStats SkillStats { get; }
        public bool IsEnabled { get; private set; }

        // 技能启用时生效的效果，比如Buff，需要关闭技能时主动 Cancel
        protected readonly List<IEffect> SkillEffectsOnEnable = new();
        // 每次使用技能时生效的效果，比如攻击，一般不需要主动 Cancel
        protected readonly List<IEffect> SkillEffectsOnUse = new();

        public Skill(T skillConfig, CharacterStats characterStats)
        {
            SkillConfig = skillConfig;
            SkillStats = new SkillStats(skillConfig.Keywords, characterStats);
        }

        public virtual void SetEffects(IEnumerable<IEffect> effectsOnEnable, IEnumerable<IEffect> effectsOnUpdate)
        {
            if (effectsOnEnable != null)
            {
                SkillEffectsOnEnable.AddRange(effectsOnEnable);
            }

            if (effectsOnUpdate != null)
            {
                SkillEffectsOnUse.AddRange(effectsOnUpdate);
            }
        }

        public virtual void Enable()
        {
            foreach (IEffect skillEffect in SkillEffectsOnEnable)
            {
                skillEffect.Enable();
                skillEffect.Apply();
            }

            foreach (IEffect skillEffect in SkillEffectsOnUse)
            {
                skillEffect.Enable();
            }

            IsEnabled = true;
        }

        public abstract void Use();

        public abstract void Cancel();

        public virtual void Disable()
        {
            foreach (IEffect skillEffect in SkillEffectsOnEnable.Concat(SkillEffectsOnUse))
            {
                skillEffect.Disable();
            }

            IsEnabled = false;
        }
    }

    public class RepetitiveSkill : Skill<RepetitiveSkillConfig>
    {
        public IKeywordStat CooldownInverse => SkillStats.GetKeywordStat("CooldownInverse");
        public float Cooldown => 1f / CooldownInverse.Value;
        public bool IsReady => _leftTime <= 0;

        float _leftTime;

        public RepetitiveSkill(RepetitiveSkillConfig skillConfig, CharacterStats characterStats) :
            base(skillConfig, characterStats)
        {
            CooldownInverse.BaseValue = 1f / skillConfig.Cooldown;
            _leftTime = 0;
        }

        public void Update(float deltaTime)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (!IsReady)
            {
                _leftTime -= deltaTime;
            }
        }

        public override void Use()
        {
            if (!IsEnabled || !IsReady)
            {
                return;
            }

            _leftTime += Cooldown;

            foreach (IEffect skillEffect in SkillEffectsOnUse)
            {
                skillEffect.Apply();
            }
        }

        public override void Cancel()
        {
            foreach (IEffect skillEffect in SkillEffectsOnUse)
            {
                skillEffect.Cancel();
            }
        }
    }

    public class OneTimeSkill : Skill<OneTimeSkillConfig>
    {
        public OneTimeSkill(OneTimeSkillConfig skillConfig, CharacterStats characterStats) :
            base(skillConfig, characterStats)
        {
        }

        public override void Enable()
        {
            base.Enable();
            Use();
        }

        public override void Use()
        {
            if (!IsEnabled)
            {
                return;
            }

            foreach (IEffect skillEffect in SkillEffectsOnUse)
            {
                skillEffect.Apply();
            }
        }

        public override void Cancel()
        {
            if (!IsEnabled)
            {
                return;
            }

            foreach (IEffect skillEffect in SkillEffectsOnUse)
            {
                skillEffect.Cancel();
            }
        }
    }
}

