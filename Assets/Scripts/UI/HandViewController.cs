using System;
using System.Collections.Generic;
using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 手牌区域控制器：监听手牌事件，通过 CardViewPool 取用/归还 View，不再 Instantiate/Destroy。
    /// </summary>
    public class HandViewController : MonoBehaviour, IController
    {
        [SerializeField, Required] Transform _handContainer;
        [SerializeField] HandDropZone _handDropZone;

        readonly List<CardViewController> _cardViews = new List<CardViewController>();

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        DeckModel _deckModel;

        void OnValidate()
        {
            if(_handContainer == null)
            {
                _handContainer = transform.Find("HandContainer");
                if (_handContainer == null)
                {
                    _handContainer = new GameObject("HandContainer").transform;
                    _handContainer.SetParent(transform);
                }
            }


            if (_handDropZone == null)
            {
                _handDropZone = _handContainer.GetComponent<HandDropZone>();
                if (_handDropZone == null)
                {
                    _handDropZone = _handContainer.gameObject.AddComponent<HandDropZone>();
                }
            }
        }

        void Awake()
        {
            _deckModel = this.GetModel<DeckModel>();
        }

        void OnEnable()
        {
            this.RegisterEvent<HandRefreshedEvent>(OnHandRefreshed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<CardRemovedFromHandEvent>(OnCardRemovedFromHand).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<CardAddedToHandEvent>(OnCardAddedToHand).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnHandRefreshed(HandRefreshedEvent e)
        {
            RefreshHand();
        }

        void OnCardRemovedFromHand(CardRemovedFromHandEvent e)
        {
            if (e.HandIndex < 0 || e.HandIndex >= _cardViews.Count) return;

            CardViewController view = _cardViews[e.HandIndex];
            view.OnCardDroppedToSlot -= OnCardDroppedToSlot;
            _cardViews.RemoveAt(e.HandIndex);
            ReturnToPool(view);
        }

        void OnCardAddedToHand(CardAddedToHandEvent e)
        {
            if (e.HandIndex < 0 || e.HandIndex >= _deckModel.Hand.Count) return;

            CardData card = _deckModel.Hand[e.HandIndex];
            SpawnCardView(card);
        }

        void RefreshHand()
        {
            ClearCardViews();

            foreach (var card in _deckModel.Hand)
                SpawnCardView(card);
        }

        void SpawnCardView(CardData card)
        {
            var pool = CardViewPool.Instance;
            if (pool == null || !pool.IsReady)
            {
                Debug.LogWarning("[HandViewController] CardViewPool 未就绪，跳过生成");
                return;
            }

            var view = pool.Rent(_handContainer);
            if (view == null) return;

            view.Setup(card);
            view.OnCardDroppedToSlot += OnCardDroppedToSlot;
            _cardViews.Add(view);
            view.transform.SetSiblingIndex(_cardViews.Count - 1);
        }

        void OnCardDroppedToSlot(CardViewController cardView, int slotIndex)
        {
            int handIndex = _cardViews.IndexOf(cardView);
            bool success = this.SendCommand(new PlayCardCommand(cardView.CardData, slotIndex, handIndex));
            if (!success)
                cardView.ReturnToHand();
        }

        void ReturnToPool(CardViewController view)
        {
            CardViewPool.Instance?.Return(view);
        }

        void ClearCardViews()
        {
            foreach (var view in _cardViews)
            {
                if (view == null) continue;
                view.OnCardDroppedToSlot -= OnCardDroppedToSlot;
                ReturnToPool(view);
            }
            _cardViews.Clear();
        }

        void OnDestroy()
        {
            ClearCardViews();
        }
    }
}
