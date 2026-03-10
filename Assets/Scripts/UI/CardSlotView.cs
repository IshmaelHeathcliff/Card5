using System.Collections.Generic;
using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 出牌槽视图：显示槽位编号和当前放置的卡牌。
    /// 拖动：拖到手牌区域放回手牌，拖到另一槽位则交换。
    /// </summary>
    public class CardSlotView : MonoBehaviour, IController,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField, Range(0, 4)] int _slotIndex;
        [SerializeField] TMPro.TextMeshProUGUI _slotLabel;
        [SerializeField] TMPro.TextMeshProUGUI _cardNameText;
        [SerializeField] Image _cardArtwork;
        [SerializeField] Image _slotBackground;
        [SerializeField] GameObject _emptyIndicator;
        [SerializeField] GameObject _filledIndicator;

        [SerializeField, MinValue(5)] float _dragThreshold = 10f;

        [ShowInInspector, ReadOnly] CardData _currentCard;

        static int s_draggingSlotIndex = -1;

        Vector2 _dragStartPosition;
        Vector2 _dragPreviewTargetLocalPosition;
        GameObject _dragPreview;
        RectTransform _dragPreviewRect;
        Canvas _rootCanvas;
        RectTransform _canvasRect;
        BattleModel _battleModel;

        public int SlotIndex => _slotIndex;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Awake()
        {
            if (_slotLabel != null)
                _slotLabel.text = $"槽 {_slotIndex + 1}";

            _rootCanvas = GetComponentInParent<Canvas>();
            if (_rootCanvas != null)
                _canvasRect = _rootCanvas.GetComponent<RectTransform>();

            _battleModel = this.GetModel<BattleModel>();
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
            if (e.SlotIndex != _slotIndex) return;

            _currentCard = _battleModel.PlaySlots[_slotIndex];
            RefreshUI();
        }

        void OnCardRemoved(CardRemovedFromSlotEvent e)
        {
            if (e.SlotIndex != _slotIndex) return;
            ClearSlot();
        }

        void OnSlotsSwapped(SlotsSwappedEvent e)
        {
            if (e.SlotA != _slotIndex && e.SlotB != _slotIndex) return;

            _currentCard = _battleModel.PlaySlots[_slotIndex];
            RefreshUI();
        }

        void OnSlotsResolved(SlotEffectsResolvedEvent e) => ClearSlot();

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentCard == null) return;

            s_draggingSlotIndex = _slotIndex;
            _dragStartPosition = eventData.position;
            CreateDragPreview(eventData);
            RefreshUI();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (s_draggingSlotIndex != _slotIndex) return;
            UpdateDragPreviewPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (s_draggingSlotIndex != _slotIndex) return;

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

            foreach (var result in results)
            {
                if (result.gameObject.GetComponent<HandDropZone>() != null)
                {
                    this.SendCommand(new ReturnCardToHandCommand(fromSlot));
                    didDrop = true;
                    break;
                }

                var otherSlot = result.gameObject.GetComponent<CardSlotView>();
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

        void CreateDragPreview(PointerEventData eventData)
        {
            if (_rootCanvas == null || _currentCard == null) return;

            _dragPreview = new GameObject("SlotDragPreview");
            _dragPreview.transform.SetParent(_rootCanvas.transform, false);

            _dragPreviewRect = _dragPreview.AddComponent<RectTransform>();
            _dragPreviewRect.sizeDelta = new Vector2(120f, 160f);

            var image = _dragPreview.AddComponent<Image>();
            image.sprite = _currentCard.Artwork;
            image.raycastTarget = false;

            var canvasGroup = _dragPreview.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.9f;
            canvasGroup.blocksRaycasts = false;

            _dragPreview.transform.SetAsLastSibling();
            UpdateDragPreviewPosition(eventData);
            _dragPreviewRect.localPosition = _dragPreviewTargetLocalPosition;
        }

        void UpdateDragPreviewPosition(PointerEventData eventData)
        {
            if (_canvasRect == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect,
                eventData.position,
                eventData.pressEventCamera,
                out _dragPreviewTargetLocalPosition);
        }

        void LateUpdate()
        {
            if (s_draggingSlotIndex != _slotIndex || _dragPreviewRect == null) return;

            _dragPreviewRect.localPosition = _dragPreviewTargetLocalPosition;
        }

        void DestroyDragPreview()
        {
            if (_dragPreview != null)
            {
                Destroy(_dragPreview);
                _dragPreview = null;
                _dragPreviewRect = null;
            }
        }

        public void ClearSlot()
        {
            _currentCard = null;
            RefreshUI();
        }

        void RefreshUI()
        {
            bool isDraggingThisSlot = s_draggingSlotIndex == _slotIndex;
            bool filled = _currentCard != null && !isDraggingThisSlot;

            if (_emptyIndicator != null) _emptyIndicator.SetActive(!filled);
            if (_filledIndicator != null) _filledIndicator.SetActive(filled);

            if (_cardNameText != null)
                _cardNameText.text = filled ? _currentCard.CardName : string.Empty;

            if (_cardArtwork != null)
            {
                _cardArtwork.enabled = filled && _currentCard.Artwork != null;
                if (filled && _currentCard.Artwork != null)
                    _cardArtwork.sprite = _currentCard.Artwork;
            }
        }

        /// <summary>供外部按钮绑定：点击将本槽卡牌放回手牌</summary>
        public void OnClickReturnCard()
        {
            if (_currentCard == null) return;
            this.SendCommand(new ReturnCardToHandCommand(_slotIndex));
        }
    }
}
