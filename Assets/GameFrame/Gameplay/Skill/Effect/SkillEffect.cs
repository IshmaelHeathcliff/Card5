using System.Collections.Generic;
using Data.Config;
using Gameplay.Character;

namespace Gameplay.Skill.Effect
{
    public enum SkillEffectType
    {
        Attack, Summon, Status, Stat, Move, System
    }

    public interface IEffect
    {
        void Enable(); // 检查激活条件并激活效果，默认无条件自动激活
        void Apply(); // 应用效果，可在技能激活下可反复应用
        void Cancel(); // 取消效果，取消激活状态下应用的所有效果
        void Disable(); // 禁用效果

        string Description { get; }
        bool IsEnabled { get; }

        ISkill Owner { get; set; }
    }

    public interface IEffect<T>
    {
        void Apply(T value);
        void Cancel(T value);

        ISkill Owner { get; set; }
    }

    public interface ISkillEffect<out T> : IEffect where T : SkillEffectConfig
    {
        T SkillEffectConfig { get; }
    }

    public abstract class SkillEffect<T> : ISkillEffect<T> where T : SkillEffectConfig
    {
        public T SkillEffectConfig { get; }
        public ISkill Owner { get; set; }
        public string Description { get; protected set; }
        public ICharacterModel Model { get; set; }

        public bool IsEnabled { get; private set; }


        protected SkillEffect(T skillEffectConfig, ICharacterModel model)
        {
            SkillEffectConfig = skillEffectConfig;
            Description = skillEffectConfig.Description;
            Model = model;
        }

        public virtual void Enable()
        {
            IsEnabled = OnEnable();
        }

        public virtual void Apply()
        {
            if (IsEnabled)
            {
                OnApply();
            }
        }

        public virtual void Cancel()
        {
            if (IsEnabled)
            {
                OnCancel();
            }
        }

        public virtual void Disable()
        {
            if (IsEnabled)
            {
                OnCancel();
                OnDisable();
                IsEnabled = false;
            }
        }

        // 检测激活条件
        protected virtual bool OnEnable() => true;

        protected abstract void OnApply();
        protected abstract void OnCancel();

        // 禁用时回调，大多时候无效果
        protected virtual void OnDisable()
        {
        }

    }

    public class NestedSkillEffect<T> : SkillEffect<T> where T : NestedEffectConfig
    {
        public List<IEffect> ChildEffects { get; set; } = new();

        public NestedSkillEffect(T skillEffectConfig, ICharacterModel model, IEnumerable<IEffect> childEffects) : base(skillEffectConfig, model)
        {
            ChildEffects.AddRange(childEffects);
        }

        protected override bool OnEnable()
        {
            foreach (IEffect childEffect in ChildEffects)
            {
                childEffect.Enable();
            }
            return true;
        }

        protected override void OnApply()
        {
            foreach (IEffect childEffect in ChildEffects)
            {
                childEffect.Apply();
            }

        }

        protected override void OnCancel()
        {
            foreach (IEffect childEffect in ChildEffects)
            {
                childEffect.Cancel();
            }
        }

        protected override void OnDisable()
        {
            foreach (IEffect childEffect in ChildEffects)
            {
                childEffect.Disable();
            }
        }
    }
}
