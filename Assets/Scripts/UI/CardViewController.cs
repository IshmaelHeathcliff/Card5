using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 单张手牌视图：显示牌面信息，处理点击/拖拽出牌逻辑。
    /// </summary>
    public class CardViewController : MonoBehaviour, IController,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] TMPro.TextMeshProUGUI _nameText;
        [SerializeField] TMPro.TextMeshProUGUI _descriptionText;
        [SerializeField] TMPro.TextMeshProUGUI _costText;
        [SerializeField] Image _artwork;
        [SerializeField] Image _cardFrame;

        [ShowInInspector, ReadOnly] CardData _cardData;

        Canvas _rootCanvas;
        RectTransform _rectTransform;
        Transform _originalParent;
        Vector3 _originalPosition;
        int _originalSiblingIndex;
        bool _isDragging;

        public CardData CardData => _cardData;

        public event Action<CardViewController, int> OnCardDroppedToSlot;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _rootCanvas = GetComponentInParent<Canvas>();
        }

        public void Setup(CardData data)
        {
            _cardData = data;

            if (_nameText != null) _nameText.text = data.CardName;
            if (_costText != null) _costText.text = data.EnergyCost.ToString();
            if (_artwork != null && data.Artwork != null) _artwork.sprite = data.Artwork;

            if (_descriptionText != null)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var effect in data.Effects)
                    sb.AppendLine(effect.GetDescription());
                _descriptionText.text = sb.ToString().TrimEnd();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _originalParent = transform.parent;
            _originalPosition = transform.localPosition;
            _originalSiblingIndex = transform.GetSiblingIndex();

            transform.SetParent(_rootCanvas.transform, true);
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _rootCanvas.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 worldPos);
            transform.position = worldPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;

            int slotIndex = FindSlotUnderPointer(eventData);
            if (slotIndex >= 0)
            {
                OnCardDroppedToSlot?.Invoke(this, slotIndex);
            }
            else
            {
                ReturnToHand();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging) return;
        }

        public void ReturnToHand()
        {
            transform.SetParent(_originalParent, true);
            transform.SetSiblingIndex(_originalSiblingIndex);
            transform.localPosition = _originalPosition;
        }

        int FindSlotUnderPointer(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponent<CardSlotView>();
                if (slot != null)
                    return slot.SlotIndex;
            }
            return -1;
        }
    }
}
