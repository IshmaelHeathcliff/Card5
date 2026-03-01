using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using Data.Config;
using Gameplay.Character;
using UnityEngine;

namespace Gameplay.Skill.Effect
{
    public class FixedRepeatEffect : NestedSkillEffect<FixedRepeatEffectConfig>
    {
        CancellationTokenSource _cts;
        public FixedRepeatEffect(FixedRepeatEffectConfig skillEffectConfig, ICharacterModel model, IEnumerable<IEffect> childEffects) : base(skillEffectConfig, model, childEffects)
        {
        }

        async UniTaskVoid Repeat()
        {
            _cts?.Cancel();
            _cts = GlobalCancellation.GetCombinedTokenSource(Model.Controller as MonoBehaviour);

            try
            {
                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await UniTask.Delay(TimeSpan.FromSeconds(SkillEffectConfig.Interval), cancellationToken: _cts.Token);
                    base.OnApply();
                }
            }
            catch (OperationCanceledException)
            {
                // Debug.Log("FixedRepeatEffect 取消");
            }
        }

        protected override void OnApply()
        {
            Repeat().Forget();
        }

        protected override void OnCancel()
        {
            _cts.Cancel();
        }
    }
}
