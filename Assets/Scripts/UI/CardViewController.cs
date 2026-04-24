using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Card5
{
    /// <summary>
    /// 单张手牌视图：显示牌面信息，处理拖拽出牌、手牌排序、重抽选中逻辑。
    /// </summary>
    public class CardViewController : MonoBehaviour, IController,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] CardDisplayView _displayView;
        [SerializeField] RectTransform _rectTransform;
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] GameObject _selectionHighlight;

        [ShowInInspector, ReadOnly] CardData _cardData;

        Transform _originalParent;
        Vector3 _originalPosition;
        int _originalSiblingIndex;

        bool _isDragging;
        bool _hasInvokedDrop;

        public CardData CardData => _cardData;

        public event Action<CardViewController, int> OnCardDroppedToSlot;
        public event Action<CardViewController, int> OnHandReorder;
        public event Action<CardViewController> OnClicked;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnValidate()
        {
            if (_rectTransform == null)
                _rectTransform = gameObject.GetComponent<RectTransform>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.GetComponent<CanvasGroup>();

            if (_displayView == null)
                _displayView = gameObject.GetComponent<CardDisplayView>();
        }

        void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = gameObject.GetComponent<RectTransform>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Setup(CardData data)
        {
            _cardData = data;
            GetDisplayView().Setup(data);
        }

        public void SetRedrawSelected(bool selected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(selected);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _hasInvokedDrop = false;
            _originalParent = transform.parent;
            _originalPosition = transform.localPosition;
            _originalSiblingIndex = transform.GetSiblingIndex();

            SetRaycastBlock(false);
            UILayerManager.MoveToLayer(transform, UILayer.Drag, true);
            MoveToPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            MoveToPointer(eventData);
        }

        void MoveToPointer(PointerEventData eventData)
        {
            var parentRect = _rectTransform.parent as RectTransform;
            if (parentRect == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint);

            _rectTransform.localPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            SetRaycastBlock(true);

            if (_hasInvokedDrop)
                return;

            int slotIndex = FindSlotUnderPointer(eventData);
            if (slotIndex >= 0)
            {
                _hasInvokedDrop = true;
                OnCardDroppedToSlot?.Invoke(this, slotIndex);
            }
            else
            {
                int newSiblingIndex = ComputeSiblingIndexFromX(eventData);
                RestoreOriginalParent(true);
                if (OnHandReorder != null)
                    OnHandReorder.Invoke(this, newSiblingIndex);
                else
                    ReturnToHand();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging) return;
            OnClicked?.Invoke(this);
        }

        public void ReturnToHand()
        {
            ResetDragState();
            RestoreOriginalParent(true);
        }

        /// <summary>重置拖拽相关状态，取用/归还对象池时调用</summary>
        public void ResetDragState()
        {
            _isDragging = false;
            _hasInvokedDrop = false;
            SetRaycastBlock(true);
            SetRedrawSelected(false);
        }

        int FindSlotUnderPointer(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject.GetComponentInParent<CardViewController>() != null)
                    continue;

                var slot = result.gameObject.GetComponentInParent<CardSlotView>();
                if (slot != null)
                    return slot.SlotIndex;
            }
            return -1;
        }

        /// <summary>根据放下位置的 X 坐标，计算在手牌容器中应插入的 sibling index</summary>
        int ComputeSiblingIndexFromX(PointerEventData eventData)
        {
            var parentRect = _originalParent as RectTransform;
            if (parentRect == null) return _originalSiblingIndex;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out var localPos);

            int insertPos = 0;
            for (int i = 0; i < _originalParent.childCount; i++)
            {
                Transform child = _originalParent.GetChild(i);
                if (child == transform) continue;
                if (child.localPosition.x < localPos.x)
                    insertPos++;
            }
            return insertPos;
        }

        void RestoreOriginalParent(bool worldPositionStays)
        {
            if (_originalParent == null) return;

            transform.SetParent(_originalParent, worldPositionStays);
            transform.SetSiblingIndex(_originalSiblingIndex);
            transform.localPosition = _originalPosition;
        }

        void SetRaycastBlock(bool blocksRaycasts)
        {
            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = blocksRaycasts;
        }

        CardDisplayView GetDisplayView()
        {
            if (_displayView == null)
            {
                _displayView = gameObject.GetComponent<CardDisplayView>();
                if (_displayView == null)
                    _displayView = gameObject.AddComponent<CardDisplayView>();
            }

            return _displayView;
        }
    }
}
