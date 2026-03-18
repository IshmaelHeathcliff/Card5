using System.Collections.Generic;

namespace Card5
{
    /// <summary>
    /// 印记状态 Model：存储当前战斗中所有活跃的槽位印记和卡牌印记。
    /// </summary>
    public class MarkModel : AbstractModel
    {
        readonly Dictionary<int, List<MarkInstance>> _slotMarks = new Dictionary<int, List<MarkInstance>>();
        readonly Dictionary<string, List<MarkInstance>> _cardMarks = new Dictionary<string, List<MarkInstance>>();

        protected override void OnInit()
        {
        }

        public void ClearAll()
        {
            _slotMarks.Clear();
            _cardMarks.Clear();
        }

        // ── 槽位印记 ──────────────────────────────────────────

        public void AddSlotMark(MarkInstance mark)
        {
            if (!_slotMarks.TryGetValue(mark.SlotIndex, out var list))
            {
                list = new List<MarkInstance>();
                _slotMarks[mark.SlotIndex] = list;
            }
            list.Add(mark);
        }

        public IReadOnlyList<MarkInstance> GetSlotMarks(int slotIndex)
        {
            return _slotMarks.TryGetValue(slotIndex, out var list)
                ? list
                : System.Array.Empty<MarkInstance>();
        }

        // ── 卡牌印记 ──────────────────────────────────────────

        public void AddCardMark(MarkInstance mark)
        {
            string id = mark.TargetCard.CardId;
            if (!_cardMarks.TryGetValue(id, out var list))
            {
                list = new List<MarkInstance>();
                _cardMarks[id] = list;
            }
            list.Add(mark);
        }

        public IReadOnlyList<MarkInstance> GetCardMarks(string cardId)
        {
            return _cardMarks.TryGetValue(cardId, out var list)
                ? list
                : System.Array.Empty<MarkInstance>();
        }

        // ── 清理过期印记 ─────────────────────────────────────

        /// <summary>返回并移除所有已过期的印记</summary>
        public List<MarkInstance> RemoveExpired()
        {
            var expired = new List<MarkInstance>();

            foreach (var pair in _slotMarks)
            {
                pair.Value.RemoveAll(m =>
                {
                    if (!m.IsExpired) return false;
                    expired.Add(m);
                    return true;
                });
            }

            foreach (var pair in _cardMarks)
            {
                pair.Value.RemoveAll(m =>
                {
                    if (!m.IsExpired) return false;
                    expired.Add(m);
                    return true;
                });
            }

            return expired;
        }

        /// <summary>对所有印记执行 Tick（回合推进），之后调用 RemoveExpired 清理</summary>
        public void TickAll()
        {
            foreach (var list in _slotMarks.Values)
                foreach (var mark in list)
                    mark.Tick();

            foreach (var list in _cardMarks.Values)
                foreach (var mark in list)
                    mark.Tick();
        }
    }
}
