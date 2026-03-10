using System.Collections.Generic;
using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 手牌区域控制器：管理手牌 View 的增删、排序和重抽选中模式。
    /// </summary>
    public class HandViewController : MonoBehaviour, IController
    {
        [SerializeField, Required] Transform _handContainer;
        [SerializeField] HandDropZone _handDropZone;

        [Title("重抽面板")]
        [SerializeField] GameObject _redrawPanel;
        [SerializeField] Button _confirmRedrawButton;
        [SerializeField] Button _cancelRedrawButton;

        readonly List<CardViewController> _cardViews = new List<CardViewController>();
        readonly HashSet<CardViewController> _redrawSelected = new HashSet<CardViewController>();

        bool _isRedrawMode;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        DeckModel _deckModel;

        void OnValidate()
        {
            if (_handContainer == null)
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
                    _handDropZone = _handContainer.gameObject.AddComponent<HandDropZone>();
            }
        }

        void Awake()
        {
            _deckModel = this.GetModel<DeckModel>();
        }

        void Start()
        {
            if (_confirmRedrawButton != null)
                _confirmRedrawButton.onClick.AddListener(ConfirmRedraw);
            if (_cancelRedrawButton != null)
                _cancelRedrawButton.onClick.AddListener(ExitRedrawMode);

            if (_redrawPanel != null)
                _redrawPanel.SetActive(false);
        }

        void OnDestroy()
        {
            if (_confirmRedrawButton != null)
                _confirmRedrawButton.onClick.RemoveListener(ConfirmRedraw);
            if (_cancelRedrawButton != null)
                _cancelRedrawButton.onClick.RemoveListener(ExitRedrawMode);

            ClearCardViews();
        }

        void OnEnable()
        {
            this.RegisterEvent<HandRefreshedEvent>(OnHandRefreshed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<CardRemovedFromHandEvent>(OnCardRemovedFromHand).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<CardAddedToHandEvent>(OnCardAddedToHand).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<TurnStartedEvent>(OnTurnStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        // ── 事件处理 ──────────────────────────────────────────

        void OnHandRefreshed(HandRefreshedEvent e)
        {
            ExitRedrawMode();
            RefreshHand();
        }

        void OnTurnStarted(TurnStartedEvent e)
        {
            ExitRedrawMode();
        }

        void OnCardRemovedFromHand(CardRemovedFromHandEvent e)
        {
            if (e.HandIndex < 0 || e.HandIndex >= _cardViews.Count) return;

            CardViewController view = _cardViews[e.HandIndex];
            UnsubscribeCard(view);
            _cardViews.RemoveAt(e.HandIndex);
            ReturnToPool(view);
        }

        void OnCardAddedToHand(CardAddedToHandEvent e)
        {
            if (e.HandIndex < 0 || e.HandIndex >= _deckModel.Hand.Count) return;

            CardData card = _deckModel.Hand[e.HandIndex];
            SpawnCardView(card);
        }

        // ── 手牌排序 ──────────────────────────────────────────

        void OnCardHandReorder(CardViewController cardView, int newSiblingIndex)
        {
            int oldIndex = _cardViews.IndexOf(cardView);
            if (oldIndex < 0) return;

            newSiblingIndex = Mathf.Clamp(newSiblingIndex, 0, _cardViews.Count - 1);

            var cardData = _deckModel.Hand[oldIndex];
            _cardViews.RemoveAt(oldIndex);
            _deckModel.Hand.RemoveAt(oldIndex);

            _cardViews.Insert(newSiblingIndex, cardView);
            _deckModel.Hand.Insert(newSiblingIndex, cardData);

            for (int i = 0; i < _cardViews.Count; i++)
                _cardViews[i].transform.SetSiblingIndex(i);

            cardView.ResetDragState();

            var handRect = _handContainer as RectTransform;
            if (handRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(handRect);
        }

        // ── 重抽选中模式 ──────────────────────────────────────

        public void EnterRedrawMode()
        {
            if (_isRedrawMode) return;
            _isRedrawMode = true;
            _redrawSelected.Clear();

            if (_redrawPanel != null)
                _redrawPanel.SetActive(true);
        }

        public void ExitRedrawMode()
        {
            if (!_isRedrawMode) return;
            _isRedrawMode = false;

            foreach (var view in _cardViews)
                view.SetRedrawSelected(false);
            _redrawSelected.Clear();

            if (_redrawPanel != null)
                _redrawPanel.SetActive(false);
        }

        void ConfirmRedraw()
        {
            if (_redrawSelected.Count == 0)
            {
                ExitRedrawMode();
                return;
            }

            var indices = new List<int>();
            foreach (var view in _redrawSelected)
            {
                int idx = _cardViews.IndexOf(view);
                if (idx >= 0) indices.Add(idx);
            }

            this.SendCommand(new RedrawCardsCommand(indices));
            ExitRedrawMode();
        }

        void OnCardClicked(CardViewController cardView)
        {
            if (!_isRedrawMode) return;

            if (_redrawSelected.Contains(cardView))
            {
                _redrawSelected.Remove(cardView);
                cardView.SetRedrawSelected(false);
            }
            else
            {
                _redrawSelected.Add(cardView);
                cardView.SetRedrawSelected(true);
            }
        }

        // ── 出牌 ──────────────────────────────────────────────

        void OnCardDroppedToSlot(CardViewController cardView, int slotIndex)
        {
            int handIndex = _cardViews.IndexOf(cardView);
            bool success = this.SendCommand(new PlayCardCommand(cardView.CardData, slotIndex, handIndex));
            if (!success)
                cardView.ReturnToHand();
        }

        // ── 内部工具 ──────────────────────────────────────────

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
            SubscribeCard(view);
            _cardViews.Add(view);
            view.transform.SetSiblingIndex(_cardViews.Count - 1);
        }

        void SubscribeCard(CardViewController view)
        {
            view.OnCardDroppedToSlot += OnCardDroppedToSlot;
            view.OnHandReorder += OnCardHandReorder;
            view.OnClicked += OnCardClicked;
        }

        void UnsubscribeCard(CardViewController view)
        {
            view.OnCardDroppedToSlot -= OnCardDroppedToSlot;
            view.OnHandReorder -= OnCardHandReorder;
            view.OnClicked -= OnCardClicked;
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
                UnsubscribeCard(view);
                ReturnToPool(view);
            }
            _cardViews.Clear();
        }
    }
}
