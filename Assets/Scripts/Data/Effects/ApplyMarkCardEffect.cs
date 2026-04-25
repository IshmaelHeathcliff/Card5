using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public class ApplyMarkCardEffect : CardEffect
    {
        [SerializeField, LabelText("印记"), Required] MarkData _mark;
        [SerializeField, LabelText("施加目标"), Tooltip("印记施加到哪个目标")]
        MarkApplyTarget _applyTo = MarkApplyTarget.CurrentSlot;

        public override void Execute(BattleContext context)
        {
            MarkSystem markSystem = context.MarkSystem;
            if (markSystem == null || _mark == null) return;

            switch (_applyTo)
            {
                case MarkApplyTarget.CurrentSlot:
                    markSystem.ApplyMarkToSlot(_mark, context.SlotIndex);
                    break;

                case MarkApplyTarget.LeftSlot:
                    int left = context.SlotIndex - 1;
                    if (left >= 0)
                        markSystem.ApplyMarkToSlot(_mark, left);
                    break;

                case MarkApplyTarget.RightSlot:
                    int right = context.SlotIndex + 1;
                    if (right < BattleModel.SlotCount)
                        markSystem.ApplyMarkToSlot(_mark, right);
                    break;

                case MarkApplyTarget.CurrentCard:
                    markSystem.ApplyMarkToCard(_mark, context.CurrentCard);
                    break;
            }
        }

        public override string GetDescription()
        {
            if (_mark == null) return "施加印记";

            string targetStr = _applyTo switch
            {
                MarkApplyTarget.CurrentSlot => "当前槽位",
                MarkApplyTarget.LeftSlot    => "左侧槽位",
                MarkApplyTarget.RightSlot   => "右侧槽位",
                MarkApplyTarget.CurrentCard => "本张卡牌",
                _                           => "目标"
            };

            return $"对{targetStr}施加印记《{_mark.MarkName}》";
        }
    }

    public enum MarkApplyTarget
    {
        /// <summary>施加到当前出牌所在的槽位</summary>
        [InspectorName("当前槽位")]
        CurrentSlot,
        /// <summary>施加到左侧相邻槽位</summary>
        [InspectorName("左侧槽位")]
        LeftSlot,
        /// <summary>施加到右侧相邻槽位</summary>
        [InspectorName("右侧槽位")]
        RightSlot,
        /// <summary>施加到当前卡牌本身，跟随牌组</summary>
        [InspectorName("当前卡牌")]
        CurrentCard
    }
}
