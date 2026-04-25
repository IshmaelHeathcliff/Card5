using System;
using System.Collections.Generic;
using Card5.Gameplay.Events;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 手牌区域控制器：管理手牌视图、拖拽换位、自动出牌和手牌布局动画。
    /// </summary>
    public class HandViewController : MonoBehaviour, IController
    {
        [SerializeField, Required] Transform _handContainer;
        [SerializeField] HandDropZone _handDropZone;

        [Title("重抽面板")]
        [SerializeField] GameObject _redrawPanel;
        [SerializeField] Button _confirmRedrawButton;
        [SerializeField] Button _cancelRedrawButton;

        [Title("手牌布局")]
        [SerializeField, MinValue(0f)] float _preferredSpacing = 150f;
        [SerializeField, MinValue(0f)] float _cardMoveDuration = 0.18f;
        [SerializeField, MinValue(0f)] float _ghostMoveDuration = 0.2f;
        [SerializeField, MinValue(0f)] float _hoverLift = 28f;
        [SerializeField, MinValue(0f)] float _hoverNeighborOffset = 40f;
        [SerializeField, MinValue(0f)] float _hoverOuterOffset = 16f;
        [SerializeField, Range(0.5f, 1.2f)] float _baseCardScale = 0.8f;
        [SerializeField, Range(0.5f, 1.2f)] float _hoverCardScale = 0.9f;
        [SerializeField, Range(0f, 20f)] float _fanAngle = 6f;
        [SerializeField, MinValue(0f)] float _slotGhostScale = 0.62f;

        readonly List<CardViewController> _cardViews = new List<CardViewController>();
        readonly HashSet<CardViewController> _redrawSelected = new HashSet<CardViewController>();
        readonly Dictionary<int, CardSlotView> _slotViews = new Dictionary<int, CardSlotView>();

        bool _isRedrawMode;
        CardViewController _hoveredCard;
        CardViewController _draggingCard;
        int _dragPreviewIndex = -1;

        DeckModel _deckModel;
        BattleModel _battleModel;
        Canvas _rootCanvas;
        RectTransform _handContainerRect;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

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
            _battleModel = this.GetModel<BattleModel>();
            _rootCanvas = GetComponentInParent<Canvas>();
            _handContainerRect = _handContainer as RectTransform;
            CacheSlotViews();
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
            this.RegisterEvent<CardReturnedToHandEvent>(OnCardReturnedToHand).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<HandSlotSwappedEvent>(OnHandSlotSwapped).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<TurnStartedEvent>(OnTurnStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleStartedEvent>(OnBattleStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnBattleStarted(BattleStartedEvent e)
        {
            _hoveredCard = null;
            _draggingCard = null;
            _dragPreviewIndex = -1;
        }

        void OnHandRefreshed(HandRefreshedEvent e)
        {
            ExitRedrawMode();
            RefreshHand(true);
        }

        void OnTurnStarted(TurnStartedEvent e)
        {
            ExitRedrawMode();
        }

        void OnCardRemovedFromHand(CardRemovedFromHandEvent e)
        {
            if (e.HandIndex < 0 || e.HandIndex >= _cardViews.Count)
                return;

            CardViewController view = _cardViews[e.HandIndex];
            if (_hoveredCard == view)
                _hoveredCard = null;
            if (_draggingCard == view)
            {
                _draggingCard = null;
                _dragPreviewIndex = -1;
            }

            UnsubscribeCard(view);
            _cardViews.RemoveAt(e.HandIndex);
            ReturnToPool(view);
            RefreshHandLayout(true);
        }

        void OnCardAddedToHand(CardAddedToHandEvent e)
        {
            if (e.HandIndex < 0 || e.HandIndex > _deckModel.Hand.Count - 1)
                return;

            CardData card = _deckModel.Hand[e.HandIndex];
            SpawnCardView(card, e.HandIndex, null);
            RefreshHandLayout(true);
        }

        void OnCardReturnedToHand(CardReturnedToHandEvent e)
        {
            if (e.HandIndex < 0 || e.HandIndex > _deckModel.Hand.Count - 1)
                return;

            CardData card = _deckModel.Hand[e.HandIndex];
            Vector3? startWorldPosition = TryGetSlotWorldPosition(e.SourceSlotIndex, out Vector3 worldPosition)
                ? worldPosition
                : null;

            CardDisplayMode displayMode = startWorldPosition.HasValue ? CardDisplayMode.Compact : CardDisplayMode.Full;
            CardViewController view = SpawnCardView(card, e.HandIndex, startWorldPosition, displayMode);
            RefreshHandLayout(true);

            if (view != null && startWorldPosition.HasValue)
                RestoreFullDisplayAfterMoveAsync(view, card).Forget();
        }

        void OnHandSlotSwapped(HandSlotSwappedEvent e)
        {
            if (e.HandIndex < 0 || e.HandIndex >= _cardViews.Count)
                return;

            CardViewController view = _cardViews[e.HandIndex];
            CardData card = _deckModel.Hand[e.HandIndex];
            view.Setup(card, CardDisplayMode.Compact);

            if (TryGetSlotWorldPosition(e.SlotIndex, out Vector3 slotWorldPosition))
                view.transform.position = slotWorldPosition;

            RefreshHandLayout(true);
            RestoreFullDisplayAfterMoveAsync(view, card).Forget();
        }

        public void EnterRedrawMode()
        {
            if (_isRedrawMode)
                return;

            _isRedrawMode = true;
            _redrawSelected.Clear();

            if (_redrawPanel != null)
                _redrawPanel.SetActive(true);
        }

        public void ExitRedrawMode()
        {
            if (!_isRedrawMode)
                return;

            _isRedrawMode = false;

            foreach (CardViewController view in _cardViews)
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
            foreach (CardViewController view in _redrawSelected)
            {
                int idx = _cardViews.IndexOf(view);
                if (idx >= 0)
                    indices.Add(idx);
            }

            this.SendCommand(new RedrawCardsCommand(indices));
            ExitRedrawMode();
        }

        void OnCardClicked(CardViewController cardView)
        {
            if (!_isRedrawMode)
                return;

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

        void OnCardRightClicked(CardViewController cardView)
        {
            if (_isRedrawMode)
                return;

            int handIndex = _cardViews.IndexOf(cardView);
            if (handIndex < 0)
                return;

            int slotIndex = FindLeftmostEmptySlot();
            if (slotIndex < 0)
                return;

            Vector3 sourceWorldPosition = cardView.RectTransform.position;
            if (_slotViews.TryGetValue(slotIndex, out CardSlotView slotView) && slotView != null)
                slotView.DelayNextReveal(_ghostMoveDuration);

            bool success = this.SendCommand(new PlayCardCommand(cardView.CardData, slotIndex, handIndex));
            if (!success)
            {
                if (slotView != null)
                    slotView.DelayNextReveal(0f);
                return;
            }

            PlayGhostToSlot(cardView.CardData, sourceWorldPosition, slotIndex).Forget();
        }

        void OnCardHoverChanged(CardViewController cardView, bool hovered)
        {
            if (_draggingCard != null)
                return;

            CardViewController nextHoveredCard = hovered ? cardView : (_hoveredCard == cardView ? null : _hoveredCard);
            if (_hoveredCard == nextHoveredCard)
                return;

            _hoveredCard = nextHoveredCard;
            RefreshHandLayout(true);
        }

        void OnCardDragStarted(CardViewController cardView)
        {
            _hoveredCard = null;
            _draggingCard = cardView;
            _dragPreviewIndex = Mathf.Max(0, _cardViews.IndexOf(cardView));
            RefreshHandLayout(true);
        }

        void OnCardDragging(CardViewController cardView, PointerEventData eventData)
        {
            if (_draggingCard != cardView)
                return;

            _hoveredCard = null;
            int previewIndex = ComputePreviewIndex(eventData);
            if (previewIndex == _dragPreviewIndex)
                return;

            _dragPreviewIndex = previewIndex;
            RefreshHandLayout(true);
        }

        void OnCardHandReorder(CardViewController cardView, int newSiblingIndex)
        {
            int oldIndex = _cardViews.IndexOf(cardView);
            if (oldIndex < 0)
                return;

            int targetIndex = _dragPreviewIndex >= 0 ? _dragPreviewIndex : newSiblingIndex;
            targetIndex = Mathf.Clamp(targetIndex, 0, _cardViews.Count - 1);

            cardView.ReturnToHandFromCurrentPosition();
            _draggingCard = null;
            _dragPreviewIndex = -1;

            if (targetIndex == oldIndex)
            {
                RefreshHandLayout(true);
                return;
            }

            CardData cardData = _deckModel.Hand[oldIndex];
            _cardViews.RemoveAt(oldIndex);
            _deckModel.Hand.RemoveAt(oldIndex);

            _cardViews.Insert(targetIndex, cardView);
            _deckModel.Hand.Insert(targetIndex, cardData);

            RefreshHandLayout(true);
        }

        void OnCardDroppedToSlot(CardViewController cardView, int slotIndex)
        {
            int handIndex = _cardViews.IndexOf(cardView);
            if (handIndex < 0)
            {
                cardView.ReturnToHand();
                RefreshHandLayout(true);
                return;
            }

            bool hasSlotCard = _battleModel.PlaySlots[slotIndex] != null;
            Vector3 sourceWorldPosition = cardView.RectTransform.position;
            bool success;

            if (hasSlotCard)
                cardView.ReturnToHand();

            _draggingCard = null;
            _dragPreviewIndex = -1;

            if (hasSlotCard)
                success = this.SendCommand(new SwapHandWithSlotCommand(cardView.CardData, handIndex, slotIndex));
            else
                success = this.SendCommand(new PlayCardCommand(cardView.CardData, slotIndex, handIndex));

            if (!success)
            {
                cardView.ReturnToHand();
                RefreshHandLayout(true);
                return;
            }

            PlayGhostToSlot(cardView.CardData, sourceWorldPosition, slotIndex).Forget();
        }

        void RefreshHand(bool instant)
        {
            ClearCardViews();

            for (int i = 0; i < _deckModel.Hand.Count; i++)
                SpawnCardView(_deckModel.Hand[i], i, null);

            RefreshHandLayout(!instant);
        }

        CardViewController SpawnCardView(
            CardData card,
            int insertIndex,
            Vector3? startWorldPosition,
            CardDisplayMode displayMode = CardDisplayMode.Full)
        {
            CardViewPool pool = CardViewPool.Instance;
            if (pool == null || !pool.IsReady)
            {
                Debug.LogWarning("[HandViewController] CardViewPool 未就绪，跳过生成");
                return null;
            }

            CardViewController view = pool.Rent(_handContainer);
            if (view == null)
                return null;

            view.Setup(card, displayMode);
            SubscribeCard(view);

            if (insertIndex < 0 || insertIndex > _cardViews.Count)
                _cardViews.Add(view);
            else
                _cardViews.Insert(insertIndex, view);

            if (startWorldPosition.HasValue)
                view.transform.position = startWorldPosition.Value;

            view.transform.SetSiblingIndex(_cardViews.Count - 1);
            return view;
        }

        void SubscribeCard(CardViewController view)
        {
            view.OnCardDroppedToSlot += OnCardDroppedToSlot;
            view.OnHandReorder += OnCardHandReorder;
            view.OnClicked += OnCardClicked;
            view.OnRightClicked += OnCardRightClicked;
            view.OnHoverChanged += OnCardHoverChanged;
            view.OnDragStarted += OnCardDragStarted;
            view.OnDragging += OnCardDragging;
        }

        void UnsubscribeCard(CardViewController view)
        {
            view.OnCardDroppedToSlot -= OnCardDroppedToSlot;
            view.OnHandReorder -= OnCardHandReorder;
            view.OnClicked -= OnCardClicked;
            view.OnRightClicked -= OnCardRightClicked;
            view.OnHoverChanged -= OnCardHoverChanged;
            view.OnDragStarted -= OnCardDragStarted;
            view.OnDragging -= OnCardDragging;
        }

        void ReturnToPool(CardViewController view)
        {
            CardViewPool.Instance?.Return(view);
        }

        void ClearCardViews()
        {
            foreach (CardViewController view in _cardViews)
            {
                if (view == null)
                    continue;

                UnsubscribeCard(view);
                ReturnToPool(view);
            }

            _cardViews.Clear();
        }

        void RefreshHandLayout(bool animated)
        {
            if (_handContainerRect == null)
                _handContainerRect = _handContainer as RectTransform;

            if (_handContainerRect == null)
                return;

            int totalCount = _cardViews.Count;
            if (totalCount == 0)
                return;

            List<CardViewController> layoutViews = BuildLayoutViewList();
            int previewIndex = _draggingCard != null ? Mathf.Clamp(_dragPreviewIndex, 0, totalCount - 1) : -1;

            float spacing = ComputeSpacing(totalCount);
            float startX = -spacing * (totalCount - 1) * 0.5f;
            int hoveredIndex = _draggingCard == null && _hoveredCard != null ? _cardViews.IndexOf(_hoveredCard) : -1;

            for (int i = 0; i < layoutViews.Count; i++)
            {
                CardViewController view = layoutViews[i];
                int displayIndex = previewIndex >= 0 && i >= previewIndex ? i + 1 : i;

                Vector3 localPosition = new Vector3(
                    startX + spacing * displayIndex + GetHoverOffset(displayIndex, hoveredIndex),
                    displayIndex == hoveredIndex ? _hoverLift : 0f,
                    0f);
                Vector3 localScale = Vector3.one * (displayIndex == hoveredIndex ? _hoverCardScale : _baseCardScale);
                float rotation = GetRotation(displayIndex, totalCount);

                view.transform.SetParent(_handContainer, true);
                view.transform.SetSiblingIndex(displayIndex);
                view.AnimateToLocalPose(localPosition, localScale, rotation, !animated, _cardMoveDuration);
            }

            if (_hoveredCard != null && _draggingCard == null)
                _hoveredCard.transform.SetAsLastSibling();
        }

        List<CardViewController> BuildLayoutViewList()
        {
            var result = new List<CardViewController>(_cardViews.Count);
            foreach (CardViewController view in _cardViews)
            {
                if (view == _draggingCard)
                    continue;

                result.Add(view);
            }

            return result;
        }

        int ComputePreviewIndex(PointerEventData eventData)
        {
            if (_handContainerRect == null)
                return 0;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _handContainerRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                return 0;
            }

            float bestBoundary = float.NegativeInfinity;
            int insertIndex = 0;

            for (int i = 0; i < _cardViews.Count; i++)
            {
                CardViewController view = _cardViews[i];
                if (view == _draggingCard)
                    continue;

                float boundary = view.RectTransform.localPosition.x;
                if (localPoint.x >= boundary)
                {
                    bestBoundary = boundary;
                    insertIndex++;
                }
            }

            if (bestBoundary == float.NegativeInfinity && localPoint.x < 0f)
                return 0;

            return Mathf.Clamp(insertIndex, 0, _cardViews.Count - 1);
        }

        float ComputeSpacing(int cardCount)
        {
            if (cardCount <= 1)
                return 0f;

            float availableWidth = Mathf.Max(0f, _handContainerRect.rect.width - 40f);
            float layoutSpacing = availableWidth / (cardCount - 1);
            return Mathf.Min(_preferredSpacing, layoutSpacing);
        }

        float GetHoverOffset(int cardIndex, int hoveredIndex)
        {
            if (hoveredIndex < 0)
                return 0f;

            int distance = Mathf.Abs(cardIndex - hoveredIndex);
            if (distance == 1)
                return cardIndex < hoveredIndex ? -_hoverNeighborOffset : _hoverNeighborOffset;
            if (distance == 2)
                return cardIndex < hoveredIndex ? -_hoverOuterOffset : _hoverOuterOffset;

            return 0f;
        }

        float GetRotation(int cardIndex, int totalCount)
        {
            if (totalCount <= 1)
                return 0f;

            float t = totalCount == 1 ? 0.5f : cardIndex / (float)(totalCount - 1);
            return Mathf.Lerp(_fanAngle, -_fanAngle, t);
        }

        int FindLeftmostEmptySlot()
        {
            for (int i = 0; i < BattleModel.SlotCount; i++)
            {
                if (_battleModel.PlaySlots[i] == null)
                    return i;
            }

            return -1;
        }

        async UniTaskVoid PlayGhostToSlot(CardData card, Vector3 sourceWorldPosition, int slotIndex)
        {
            CardViewPool pool = CardViewPool.Instance;
            if (pool == null || !pool.IsReady)
                return;

            if (!TryGetSlotWorldPosition(slotIndex, out Vector3 targetWorldPosition))
                return;

            RectTransform dragLayer = GetDragLayer();
            if (dragLayer == null)
                return;

            CardViewController ghost = pool.Rent(dragLayer);
            if (ghost == null)
                return;

            ghost.Setup(card, CardDisplayMode.Compact);
            ghost.SetInteractionEnabled(false);
            ghost.transform.position = sourceWorldPosition;
            ghost.transform.SetAsLastSibling();

            Vector3 targetLocalPosition = dragLayer.InverseTransformPoint(targetWorldPosition);
            ghost.AnimateToLocalPose(
                targetLocalPosition,
                Vector3.one * _slotGhostScale,
                0f,
                false,
                _ghostMoveDuration);

            await UniTask.Delay(TimeSpan.FromSeconds(_ghostMoveDuration));

            ghost.SetInteractionEnabled(true);
            ReturnToPool(ghost);
        }

        async UniTaskVoid RestoreFullDisplayAfterMoveAsync(CardViewController view, CardData card)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_cardMoveDuration));

            if (view == null || !view.gameObject.activeInHierarchy)
                return;
            if (view.CardData != card)
                return;

            view.SetDisplayMode(CardDisplayMode.Full);
        }

        RectTransform GetDragLayer()
        {
            if (_rootCanvas == null)
                _rootCanvas = GetComponentInParent<Canvas>();

            return _rootCanvas != null ? UILayerManager.GetLayer(_rootCanvas, UILayer.Drag) : null;
        }

        void CacheSlotViews()
        {
            _slotViews.Clear();

            CardSlotView[] slotViews = FindObjectsByType<CardSlotView>();
            foreach (CardSlotView slotView in slotViews)
            {
                if (_slotViews.ContainsKey(slotView.SlotIndex))
                    continue;

                _slotViews.Add(slotView.SlotIndex, slotView);
            }
        }

        bool TryGetSlotWorldPosition(int slotIndex, out Vector3 worldPosition)
        {
            if (_slotViews.Count == 0)
                CacheSlotViews();

            if (_slotViews.TryGetValue(slotIndex, out CardSlotView slotView) && slotView != null)
            {
                worldPosition = slotView.GetCardAnchorWorldPosition();
                return true;
            }

            worldPosition = Vector3.zero;
            return false;
        }
    }
}
