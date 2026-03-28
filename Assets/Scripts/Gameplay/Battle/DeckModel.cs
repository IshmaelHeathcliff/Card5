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

        /// <summary>将卡牌加入持久牌库和弃牌堆（下次洗牌时混入抽牌堆）</summary>
        public void AddCard(CardData card)
        {
            FullDeck.Add(card);
            DiscardPile.Add(card);
        }

        /// <summary>从持久牌库和当前流通中移除一张卡牌。优先从弃牌堆移除，其次从抽牌堆。手牌中的牌不强制移除。</summary>
        public bool RemoveCard(CardData card)
        {
            if (!FullDeck.Remove(card)) return false;
            if (!DiscardPile.Remove(card))
                DrawPile.Remove(card);
            return true;
        }
    }
}
