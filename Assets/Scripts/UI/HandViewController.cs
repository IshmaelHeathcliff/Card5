using System.Collections.Generic;
using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Card5
{
    /// <summary>
    /// 手牌区域控制器：监听手牌刷新事件，动态实例化 CardViewController。
    /// </summary>
    public class HandViewController : MonoBehaviour, IController
    {
        [SerializeField, Required] AssetReferenceGameObject _cardViewPrefab;
        [SerializeField, Required] Transform _handContainer;

        readonly List<CardViewController> _cardViews = new List<CardViewController>();

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnEnable()
        {
            this.RegisterEvent<HandRefreshedEvent>(OnHandRefreshed).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnHandRefreshed(HandRefreshedEvent e)
        {
            RefreshHand();
        }

        void RefreshHand()
        {
            ClearCardViews();

            var deckModel = this.GetModel<DeckModel>();
            foreach (var card in deckModel.Hand)
            {
                SpawnCardView(card);
            }
        }

        void SpawnCardView(CardData card)
        {
            Addressables.InstantiateAsync(_cardViewPrefab, _handContainer).Completed += handle =>
            {
                var go = handle.Result;
                var view = go.GetComponent<CardViewController>();
                if (view == null)
                {
                    Addressables.ReleaseInstance(go);
                    return;
                }

                view.Setup(card);
                view.OnCardDroppedToSlot += OnCardDroppedToSlot;
                _cardViews.Add(view);
            };
        }

        void OnCardDroppedToSlot(CardViewController cardView, int slotIndex)
        {
            bool success = this.SendCommand(new PlayCardCommand(cardView.CardData, slotIndex));
            if (success)
            {
                cardView.OnCardDroppedToSlot -= OnCardDroppedToSlot;
                _cardViews.Remove(cardView);
                Addressables.ReleaseInstance(cardView.gameObject);
            }
            else
            {
                cardView.ReturnToHand();
            }
        }

        void ClearCardViews()
        {
            foreach (var view in _cardViews)
            {
                if (view == null) continue;
                view.OnCardDroppedToSlot -= OnCardDroppedToSlot;
                Addressables.ReleaseInstance(view.gameObject);
            }
            _cardViews.Clear();
        }

        void OnDestroy()
        {
            ClearCardViews();
        }
    }
}
