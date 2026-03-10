using System.Collections.Generic;
using Card5.Gameplay.Events;
using UnityEngine;

namespace Card5
{
    /// <summary>
    /// 卡牌系统：负责抽牌、洗牌逻辑。
    /// </summary>
    public class CardSystem : AbstractSystem
    {
        protected override void OnInit()
        {
        }

        /// <summary>从抽牌堆顶抽一张牌到手牌，若抽牌堆为空则先将弃牌堆洗入</summary>
        public bool DrawCard()
        {
            var deckModel = this.GetModel<DeckModel>();

            if (deckModel.DrawPile.Count == 0)
            {
                if (deckModel.DiscardPile.Count == 0)
                    return false;

                deckModel.ReshuffleDiscardIntoDraw();
                Shuffle(deckModel.DrawPile);
            }

            var card = deckModel.DrawPile[0];
            deckModel.DrawPile.RemoveAt(0);
            deckModel.Hand.Add(card);

            int handIndex = deckModel.Hand.Count - 1;
            this.SendEvent(new CardDrawnEvent { CardId = card.CardId, HandIndex = handIndex });
            this.SendEvent(new CardAddedToHandEvent { HandIndex = handIndex });

            return true;
        }

        /// <summary>抽多张牌</summary>
        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!DrawCard()) break;
            }
        }

        /// <summary>将手牌全部移至弃牌堆</summary>
        public void DiscardHand()
        {
            var deckModel = this.GetModel<DeckModel>();
            deckModel.MoveHandToDiscard();

            this.SendEvent(new HandRefreshedEvent { CardIds = new List<string>() });
        }

        /// <summary>将指定手牌移至弃牌堆</summary>
        public void DiscardCard(CardData card)
        {
            var deckModel = this.GetModel<DeckModel>();
            int handIndex = deckModel.Hand.IndexOf(card);
            if (handIndex < 0) return;
            deckModel.Hand.RemoveAt(handIndex);
            deckModel.DiscardPile.Add(card);

            this.SendEvent(new CardRemovedFromHandEvent { HandIndex = handIndex });
        }

        /// <summary>丢弃指定索引的手牌（降序处理避免移除时索引偏移），并重新抽取相同数量的牌</summary>
        public void RedrawCards(List<int> handIndices)
        {
            if (handIndices == null || handIndices.Count == 0) return;

            var deckModel = this.GetModel<DeckModel>();
            var sortedIndices = new List<int>(handIndices);
            sortedIndices.Sort((a, b) => b.CompareTo(a));

            int count = 0;
            foreach (int idx in sortedIndices)
            {
                if (idx < 0 || idx >= deckModel.Hand.Count) continue;
                var card = deckModel.Hand[idx];
                deckModel.Hand.RemoveAt(idx);
                deckModel.DiscardPile.Add(card);
                this.SendEvent(new CardRemovedFromHandEvent { HandIndex = idx });
                count++;
            }

            DrawCards(count);
        }

        public void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
