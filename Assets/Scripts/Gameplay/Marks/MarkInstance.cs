namespace Card5
{
    public enum MarkTargetType
    {
        Slot,
        Card
    }

    /// <summary>
    /// 印记运行时实例：持有印记配置、目标和当前剩余持续时间。
    /// </summary>
    public class MarkInstance
    {
        public MarkData Data { get; }
        public MarkTargetType TargetType { get; }

        /// <summary>槽位印记目标索引（TargetType == Slot 时有效）</summary>
        public int SlotIndex { get; }

        /// <summary>卡牌印记目标（TargetType == Card 时有效）</summary>
        public CardData TargetCard { get; }

        /// <summary>剩余持续回合数，-1 表示永久</summary>
        public int RemainingDuration { get; private set; }

        public bool IsPermanent => Data.Duration < 0;
        public bool IsExpired => !IsPermanent && RemainingDuration <= 0;

        public MarkInstance(MarkData data, int slotIndex)
        {
            Data = data;
            TargetType = MarkTargetType.Slot;
            SlotIndex = slotIndex;
            RemainingDuration = data.Duration;
        }

        public MarkInstance(MarkData data, CardData targetCard)
        {
            Data = data;
            TargetType = MarkTargetType.Card;
            TargetCard = targetCard;
            RemainingDuration = data.Duration;
        }

        /// <summary>回合结束时减少持续时间</summary>
        public void Tick()
        {
            if (!IsPermanent)
                RemainingDuration--;
        }
    }
}
