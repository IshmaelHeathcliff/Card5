using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "ApplyMarkEffect", menuName = "Card5/Effects/ApplyMark")]
    public class ApplyMarkEffectSO : CardEffectSO
    {
        [SerializeField, Required] MarkData _mark;

        [SerializeField, Tooltip("印记施加到哪个目标")]
        MarkApplyTarget _applyTo = MarkApplyTarget.CurrentSlot;

        public override void Execute(BattleContext context)
        {
            var markSystem = context.MarkSystem;
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
            return $"对{targetStr}施加印记【{_mark.MarkName}】";
        }
    }

    public enum MarkApplyTarget
    {
        /// <summary>施加到当前出牌所在的槽位</summary>
        CurrentSlot,
        /// <summary>施加到左侧相邻槽位</summary>
        LeftSlot,
        /// <summary>施加到右侧相邻槽位</summary>
        RightSlot,
        /// <summary>施加到当前卡牌本身（跟随牌组）</summary>
        CurrentCard
    }
}
