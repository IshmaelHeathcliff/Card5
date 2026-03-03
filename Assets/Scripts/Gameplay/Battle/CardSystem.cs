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

            this.SendEvent(new CardDrawnEvent
            {
                CardId = card.CardId,
                HandIndex = deckModel.Hand.Count - 1
            });

            return true;
        }

        /// <summary>抽多张牌</summary>
        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!DrawCard()) break;
            }

            var deckModel = this.GetModel<DeckModel>();
            var cardIds = new List<string>();
            foreach (var card in deckModel.Hand)
                cardIds.Add(card.CardId);

            this.SendEvent(new HandRefreshedEvent { CardIds = cardIds });
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
            if (deckModel.Hand.Remove(card))
            {
                deckModel.DiscardPile.Add(card);

                var cardIds = new List<string>();
                foreach (var c in deckModel.Hand)
                    cardIds.Add(c.CardId);

                this.SendEvent(new HandRefreshedEvent { CardIds = cardIds });
            }
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
