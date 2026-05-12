using System;
using System.Collections.Generic;
using Card5.Gameplay.Events;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 出牌槽视图：显示槽位卡牌，支持拖回手牌区和槽位间交换。
    /// </summary>
    public class CardSlotView : MonoBehaviour, IController,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField, Range(0, 4)] int _slotIndex;
        [SerializeField] TMPro.TextMeshProUGUI _slotLabel;
        [SerializeField] CardDisplayView _displayView;
        [SerializeField] Image _slotBackground;
        [SerializeField] GameObject _emptyIndicator;
        [SerializeField] GameObject _filledIndicator;

        [SerializeField, MinValue(5)] float _dragThreshold = 10f;
        [SerializeField] Color _emptySlotColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        [SerializeField] Color _validPositionColor = new Color(0.2f, 0.85f, 0.35f, 1f);
        [SerializeField] Color _invalidPositionColor = new Color(1f, 0.2f, 0.2f, 1f);
        [Title("拖拽预览")]
        [SerializeField, LabelText("预览尺寸")] Vector2 _dragPreviewSize;
        [SerializeField, LabelText("预览缩放")] float _dragPreviewScale;
        [SerializeField, LabelText("预览透明度"), Range(0f, 1f)] float _dragPreviewAlpha = 0.9f;
        [SerializeField, LabelText("预览颜色")] Color _dragPreviewColor = Color.white;
        [SerializeField, LabelText("交换动画时长"), MinValue(0f)] float _swapAnimationDuration = 0.18f;
        [SerializeField, LabelText("弃牌动画时长"), MinValue(0f)] float _discardAnimationDuration = 0.22f;
        [SerializeField, LabelText("飞行动画缩放"), MinValue(0f)] float _ghostAnimationScale = 0.58f;

        [ShowInInspector, ReadOnly] CardData _currentCard;

        static int s_draggingSlotIndex = -1;

        Vector2 _dragStartPosition;
        Vector2 _dragPreviewTargetLocalPosition;
        CardViewController _dragPreviewView;
        GameObject _dragPreview;
        RectTransform _dragPreviewRect;
        RectTransform _dragPreviewParentRect;
        RectTransform _rectTransform;
        Canvas _rootCanvas;
        BattleModel _battleModel;
        HandViewController _handViewController;
        bool _isRevealPending;
        float _nextRevealDelay;
        int _revealRequestId;

        public int SlotIndex => _slotIndex;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Awake()
        {
            if (_slotLabel != null)
                _slotLabel.text = $"槽 {_slotIndex + 1}";

            _rectTransform = GetComponent<RectTransform>();
            _rootCanvas = GetComponentInParent<Canvas>();
            _battleModel = this.GetModel<BattleModel>();

            GetDisplayView();
        }

        void OnEnable()
        {
            this.RegisterEvent<CardPlayedEvent>(OnCardPlayed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<CardRemovedFromSlotEvent>(OnCardRemoved).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SlotsSwappedEvent>(OnSlotsSwapped).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SlotEffectsResolvedEvent>(OnSlotsResolved).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleStartedEvent>(OnBattleStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnDisable()
        {
            if (s_draggingSlotIndex == _slotIndex)
            {
                s_draggingSlotIndex = -1;
                DestroyDragPreview();
            }
        }

        void OnBattleStarted(BattleStartedEvent e) => ClearSlot();

        void OnCardPlayed(CardPlayedEvent e)
        {
            if (e.SlotIndex != _slotIndex)
                return;

            _currentCard = _battleModel.PlaySlots[_slotIndex];
            float revealDelay = _nextRevealDelay;
            _nextRevealDelay = 0f;

            if (revealDelay > 0f)
            {
                _isRevealPending = true;
                int requestId = ++_revealRequestId;
                RefreshUI();
                RevealCardAfterDelayAsync(requestId, revealDelay).Forget();
                return;
            }

            _isRevealPending = false;
            RefreshUI();
        }

        void OnCardRemoved(CardRemovedFromSlotEvent e)
        {
            if (e.SlotIndex != _slotIndex)
                return;

            ClearSlot();
        }

        void OnSlotsSwapped(SlotsSwappedEvent e)
        {
            if (e.SlotA != _slotIndex && e.SlotB != _slotIndex)
                return;

            bool isSourceSlot = e.SlotA == _slotIndex;
            bool isTargetSlot = e.SlotB == _slotIndex;
            int otherSlotIndex = isSourceSlot ? e.SlotB : e.SlotA;
            CardData incomingCard = _battleModel.PlaySlots[_slotIndex];
            CardData sourceSlotIncomingCard = _battleModel.PlaySlots[e.SlotA];

            _currentCard = incomingCard;
            _nextRevealDelay = 0f;

            if (isSourceSlot && incomingCard != null && TryGetSlotWorldPosition(otherSlotIndex, out Vector3 sourceWorldPosition))
            {
                _isRevealPending = true;
                PlayGhostToWorldPosition(incomingCard, sourceWorldPosition, GetCardAnchorWorldPosition(), _swapAnimationDuration).Forget();

                int requestId = ++_revealRequestId;
                RefreshUI();
                RevealCardAfterDelayAsync(requestId, _swapAnimationDuration).Forget();
                return;
            }

            if (isTargetSlot && sourceSlotIncomingCard != null && incomingCard != null)
            {
                _isRevealPending = true;

                int requestId = ++_revealRequestId;
                RefreshUI();
                RevealCardAfterDelayAsync(requestId, _swapAnimationDuration).Forget();
                return;
            }

            _isRevealPending = false;
            RefreshUI();
        }

        void OnSlotsResolved(SlotEffectsResolvedEvent e)
        {
            if (_currentCard != null)
            {
                UIPopupManager.RegisterRewardPopupBlock(_discardAnimationDuration);
                GetHandViewController()?.RegisterConcurrentDrawVisualBlock(_discardAnimationDuration);
                PlayGhostToDiscard(_currentCard, GetCardAnchorWorldPosition()).Forget();
            }

            ClearSlot();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            if (_currentCard == null)
                return;

            s_draggingSlotIndex = _slotIndex;
            _dragStartPosition = eventData.position;
            CreateDragPreview(eventData);
            RefreshUI();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            if (s_draggingSlotIndex != _slotIndex)
                return;

            UpdateDragPreviewPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            if (s_draggingSlotIndex != _slotIndex)
                return;

            Vector3 dragPreviewWorldPosition = _dragPreviewRect != null ? _dragPreviewRect.position : transform.position;
            int fromSlot = s_draggingSlotIndex;
            s_draggingSlotIndex = -1;
            DestroyDragPreview();

            float distance = Vector2.Distance(_dragStartPosition, eventData.position);
            if (distance < _dragThreshold)
            {
                RefreshUI();
                return;
            }

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            bool didDrop = false;

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponent<HandDropZone>() != null)
                {
                    HandViewController handViewController = GetHandViewController();
                    handViewController?.PrepareReturnedCardDrop(dragPreviewWorldPosition);
                    bool success = this.SendCommand(new ReturnCardToHandCommand(fromSlot));
                    if (!success)
                        handViewController?.ClearPendingReturnedCardDrop();
                    didDrop = true;
                    break;
                }

                CardSlotView otherSlot = result.gameObject.GetComponent<CardSlotView>();
                if (otherSlot != null && otherSlot.SlotIndex != fromSlot)
                {
                    this.SendCommand(new SwapSlotsCommand(fromSlot, otherSlot.SlotIndex));
                    didDrop = true;
                    break;
                }
            }

            if (!didDrop)
                RefreshUI();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            if (_currentCard == null)
                return;
            if (s_draggingSlotIndex >= 0)
                return;

            this.SendCommand(new ReturnCardToHandCommand(_slotIndex));
        }

        void CreateDragPreview(PointerEventData eventData)
        {
            if (_rootCanvas == null || _currentCard == null)
                return;

            RectTransform dragLayer = UILayerManager.GetLayer(_rootCanvas, UILayer.Drag);
            Transform previewParent = dragLayer != null ? dragLayer : _rootCanvas.transform;
            _dragPreviewParentRect = previewParent as RectTransform;
            if (!TryCreateCardViewPreview(previewParent))
                CreateImagePreview(previewParent);

            UpdateDragPreviewPosition(eventData);
            if (_dragPreviewRect != null)
                _dragPreviewRect.localPosition = _dragPreviewTargetLocalPosition;
        }

        bool TryCreateCardViewPreview(Transform previewParent)
        {
            CardViewPool pool = CardViewPool.Instance;
            if (pool == null || !pool.IsReady)
                return false;

            _dragPreviewView = pool.Rent(previewParent);
            if (_dragPreviewView == null)
                return false;

            _dragPreviewView.Setup(_currentCard);
            _dragPreviewView.SetInteractionEnabled(false);
            _dragPreviewRect = _dragPreviewView.RectTransform;

            if (_dragPreviewRect == null)
            {
                CardViewPool.Instance?.Return(_dragPreviewView);
                _dragPreviewView = null;
                return false;
            }

            if (_dragPreviewSize != Vector2.zero)
                _dragPreviewRect.sizeDelta = _dragPreviewSize;
            _dragPreviewRect.localScale = Vector3.one * _dragPreviewScale;

            CanvasGroup canvasGroup = _dragPreviewView.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = _dragPreviewAlpha;
                canvasGroup.blocksRaycasts = false;
            }

            return true;
        }

        void CreateImagePreview(Transform previewParent)
        {
            _dragPreview = new GameObject("SlotDragPreview");
            _dragPreview.transform.SetParent(previewParent, false);

            _dragPreviewRect = _dragPreview.AddComponent<RectTransform>();
            _dragPreviewRect.sizeDelta = _dragPreviewSize;
            _dragPreviewRect.localScale = Vector3.one * _dragPreviewScale;

            var image = _dragPreview.AddComponent<Image>();
            image.sprite = _currentCard.Artwork;
            image.color = _dragPreviewColor;
            image.raycastTarget = false;

            var canvasGroup = _dragPreview.AddComponent<CanvasGroup>();
            canvasGroup.alpha = _dragPreviewAlpha;
            canvasGroup.blocksRaycasts = false;
        }

        void UpdateDragPreviewPosition(PointerEventData eventData)
        {
            if (_dragPreviewParentRect == null)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _dragPreviewParentRect,
                eventData.position,
                eventData.pressEventCamera,
                out _dragPreviewTargetLocalPosition);
        }

        void LateUpdate()
        {
            if (s_draggingSlotIndex != _slotIndex || _dragPreviewRect == null)
                return;

            _dragPreviewRect.localPosition = _dragPreviewTargetLocalPosition;
        }

        void DestroyDragPreview()
        {
            if (_dragPreviewView != null)
            {
                CanvasGroup canvasGroup = _dragPreviewView.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.blocksRaycasts = true;
                }

                _dragPreviewView.SetInteractionEnabled(true);
                CardViewPool.Instance?.Return(_dragPreviewView);
                _dragPreviewView = null;
            }

            if (_dragPreview == null)
            {
                _dragPreviewRect = null;
                _dragPreviewParentRect = null;
                return;
            }

            Destroy(_dragPreview);
            _dragPreview = null;
            _dragPreviewRect = null;
            _dragPreviewParentRect = null;
        }

        public void ClearSlot()
        {
            CancelPendingReveal();
            _currentCard = null;
            RefreshUI();
        }

        public void DelayNextReveal(float delay)
        {
            _nextRevealDelay = Mathf.Max(0f, delay);
        }

        public Vector3 GetCardAnchorWorldPosition()
        {
            RectTransform displayRect = _displayView != null ? _displayView.transform as RectTransform : null;
            if (displayRect != null)
                return displayRect.TransformPoint(displayRect.rect.center);

            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform != null
                ? _rectTransform.TransformPoint(_rectTransform.rect.center)
                : transform.position;
        }

        async UniTaskVoid PlayGhostToDiscard(CardData card, Vector3 sourceWorldPosition)
        {
            if (DeckPileView.DiscardPileInstance == null)
                return;

            await PlayGhostToWorldPosition(
                card,
                sourceWorldPosition,
                DeckPileView.DiscardPileInstance.GetAnchorWorldPosition(),
                _discardAnimationDuration);
        }

        async UniTask PlayGhostToWorldPosition(
            CardData card,
            Vector3 sourceWorldPosition,
            Vector3 targetWorldPosition,
            float duration)
        {
            CardViewPool pool = CardViewPool.Instance;
            if (pool == null || !pool.IsReady)
                return;

            RectTransform dragLayer = UILayerManager.GetLayer(_rootCanvas, UILayer.Drag);
            if (dragLayer == null)
                return;

            CardViewController ghost = pool.Rent(dragLayer);
            if (ghost == null)
                return;

            ghost.Setup(card);
            ghost.SetInteractionEnabled(false);
            ghost.transform.position = sourceWorldPosition;
            ghost.transform.SetAsLastSibling();

            Vector3 targetLocalPosition = dragLayer.InverseTransformPoint(targetWorldPosition);
            ghost.AnimateToLocalPose(
                targetLocalPosition,
                Vector3.one * _ghostAnimationScale,
                0f,
                false,
                duration);

            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            ghost.SetInteractionEnabled(true);
            CardViewPool.Instance?.Return(ghost);
        }

        HandViewController GetHandViewController()
        {
            if (_handViewController == null)
                _handViewController = FindAnyObjectByType<HandViewController>();

            return _handViewController;
        }

        bool TryGetSlotWorldPosition(int slotIndex, out Vector3 worldPosition)
        {
            CardSlotView[] slotViews = FindObjectsByType<CardSlotView>();
            foreach (CardSlotView slotView in slotViews)
            {
                if (slotView == null || slotView.SlotIndex != slotIndex)
                    continue;

                worldPosition = slotView.GetCardAnchorWorldPosition();
                return true;
            }

            worldPosition = Vector3.zero;
            return false;
        }

        void RefreshUI()
        {
            bool isDraggingThisSlot = s_draggingSlotIndex == _slotIndex;
            bool filled = _currentCard != null && !isDraggingThisSlot && !_isRevealPending;
            bool invalidPosition = filled && !_currentCard.CanActivateAtSlot(_slotIndex);

            if (_emptyIndicator != null)
                _emptyIndicator.SetActive(!filled);
            if (_filledIndicator != null)
                _filledIndicator.SetActive(filled);

            if (_slotBackground != null)
                _slotBackground.color = GetSlotBackgroundColor(filled, invalidPosition);

            CardDisplayView displayView = GetDisplayView();
            if (filled)
                displayView.Setup(_currentCard);
            else
                displayView.Clear();
        }

        void CancelPendingReveal()
        {
            _isRevealPending = false;
            _nextRevealDelay = 0f;
            _revealRequestId++;
        }

        async UniTaskVoid RevealCardAfterDelayAsync(int requestId, float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

            if (requestId != _revealRequestId)
                return;
            if (_currentCard == null)
                return;

            _isRevealPending = false;
            RefreshUI();
        }

        Color GetSlotBackgroundColor(bool filled, bool invalidPosition)
        {
            if (!filled)
                return _emptySlotColor;

            return invalidPosition ? _invalidPositionColor : _validPositionColor;
        }

        public void OnClickReturnCard()
        {
            if (_currentCard == null)
                return;

            this.SendCommand(new ReturnCardToHandCommand(_slotIndex));
        }

        CardDisplayView GetDisplayView()
        {
            if (_displayView == null)
            {
                _displayView = GetComponentInChildren<CardDisplayView>();
                if (_displayView == null)
                    _displayView = gameObject.AddComponent<CardDisplayView>();
            }

            return _displayView;
        }
    }
}
