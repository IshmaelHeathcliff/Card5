using System.Collections.Generic;

namespace Card5
{
    /// <summary>
    /// 牌组状态 Model：管理完整牌组、抽牌堆、手牌、弃牌堆。
    /// </summary>
    public class DeckModel : AbstractModel
    {
        /// <summary>玩家完整牌组（配置数据）</summary>
        public List<CardData> FullDeck { get; } = new List<CardData>();

        /// <summary>当前抽牌堆（已洗牌）</summary>
        public List<CardData> DrawPile { get; } = new List<CardData>();

        /// <summary>当前手牌</summary>
        public List<CardData> Hand { get; } = new List<CardData>();

        /// <summary>弃牌堆</summary>
        public List<CardData> DiscardPile { get; } = new List<CardData>();

        protected override void OnInit()
        {
        }

        public void InitDeck(List<CardData> deckCards)
        {
            FullDeck.Clear();
            FullDeck.AddRange(deckCards);
            DrawPile.Clear();
            DrawPile.AddRange(deckCards);
            Hand.Clear();
            DiscardPile.Clear();
        }

        public bool CanDraw() => DrawPile.Count > 0 || DiscardPile.Count > 0;

        public void ReshuffleDiscardIntoDraw()
        {
            DrawPile.AddRange(DiscardPile);
            DiscardPile.Clear();
        }

        public void MoveHandToDiscard()
        {
            DiscardPile.AddRange(Hand);
            Hand.Clear();
        }
    }
}
