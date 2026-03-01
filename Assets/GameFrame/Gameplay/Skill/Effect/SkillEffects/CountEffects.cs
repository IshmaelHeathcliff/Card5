using System.Collections.Generic;
using Data.Config;
using Gameplay.Character;

namespace Gameplay.Skill.Effect
{
    public class CountIncrementEffect : NestedSkillEffect<CountIncrementEffectConfig>
    {
        int _lastTriggerValue;

        IUnRegister _unRegister;

        readonly CountSystem _countSystem;



        public CountIncrementEffect(CountIncrementEffectConfig config, ICharacterModel model, IEnumerable<IEffect> childEffects, CountSystem system) : base(config, model, childEffects)
        {
            _countSystem = system;
            Description = $"{config.CountValueID} 每 {config.Increment} 触发";
        }

        protected override void OnApply()
        {
            if (_countSystem == null)
            {
                return;
            }

            _unRegister ??= _countSystem.Register(SkillEffectConfig.CountValueID, Model, OnCountValueChanged);
            _lastTriggerValue = _countSystem.GetCount(SkillEffectConfig.CountValueID, Model); // 初始化上一次触发时的计数
        }

        protected override void OnCancel()
        {
            _unRegister?.UnRegister();
            _unRegister = null;
        }

        void OnCountValueChanged(CountChangedEvent e)
        {
            // 检查计数是否达到或超过阈值
            if (e.Value - _lastTriggerValue >= SkillEffectConfig.Increment)
            {
                foreach (IEffect childEffect in ChildEffects)
                {
                    childEffect.Apply();
                }

                _lastTriggerValue = e.Value; // 更新上一次触发时的计数
            }
        }
    }
}
