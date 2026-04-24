using System.Collections.Generic;
using Card5.Gameplay.Events;

namespace Card5
{
    /// <summary>
    /// 印记系统：负责施加印记、执行印记效果、每回合推进持续时间并清理过期印记。
    /// </summary>
    public class MarkSystem : AbstractSystem
    {
        MarkModel _markModel;

        protected override void OnInit()
        {
            _markModel = this.GetModel<MarkModel>();
        }

        // ── 施加印记 ──────────────────────────────────────────

        /// <summary>将印记施加到指定槽位</summary>
        public void ApplyMarkToSlot(MarkData data, int slotIndex)
        {
            var instance = new MarkInstance(data, slotIndex);
            _markModel.AddSlotMark(instance);

            this.SendEvent(new MarkAppliedEvent
            {
                MarkId = data.MarkId,
                TargetType = MarkTargetType.Slot,
                SlotIndex = slotIndex
            });
        }

        /// <summary>将印记施加到指定卡牌（跟随牌组，每次该牌被结算时触发）</summary>
        public void ApplyMarkToCard(MarkData data, CardData card)
        {
            var instance = new MarkInstance(data, card);
            _markModel.AddCardMark(instance);

            this.SendEvent(new MarkAppliedEvent
            {
                MarkId = data.MarkId,
                TargetType = MarkTargetType.Card,
                CardId = card.CardId
            });
        }

        // ── 执行印记效果 ──────────────────────────────────────

        /// <summary>执行指定槽位上匹配触发时机的印记效果</summary>
        public void ExecuteSlotMarks(int slotIndex, MarkTrigger trigger, BattleContext context)
        {
            var marks = _markModel.GetSlotMarks(slotIndex);
            ExecuteMarks(marks, trigger, context);
        }

        /// <summary>执行指定卡牌上匹配触发时机的印记效果</summary>
        public void ExecuteCardMarks(CardData card, MarkTrigger trigger, BattleContext context)
        {
            var marks = _markModel.GetCardMarks(card.CardId);
            ExecuteMarks(marks, trigger, context);
        }

        void ExecuteMarks(IReadOnlyList<MarkInstance> marks, MarkTrigger trigger, BattleContext context)
        {
            foreach (var mark in marks)
            {
                if (mark.Data.Trigger != trigger) continue;

                foreach (var effect in mark.Data.Effects)
                {
                    if (effect == null) continue;

                    effect.Execute(context);
                }
            }
        }

        // ── 回合推进与清理 ────────────────────────────────────

        /// <summary>每回合开始时调用：tick 所有印记，移除到期者并发送事件</summary>
        public void TickMarks()
        {
            _markModel.TickAll();

            var expired = _markModel.RemoveExpired();
            foreach (var mark in expired)
            {
                var evt = new MarkRemovedEvent { MarkId = mark.Data.MarkId, TargetType = mark.TargetType };
                if (mark.TargetType == MarkTargetType.Slot)
                    evt.SlotIndex = mark.SlotIndex;
                else
                    evt.CardId = mark.TargetCard?.CardId ?? string.Empty;

                this.SendEvent(evt);
            }
        }

        /// <summary>战斗结束时清理所有印记</summary>
        public void ClearAllMarks()
        {
            _markModel.ClearAll();
        }
    }
}
