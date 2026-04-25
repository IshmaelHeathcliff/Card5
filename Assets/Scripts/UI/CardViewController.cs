using System;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Card5
{
    /// <summary>
    /// 单张卡牌视图：显示卡牌信息，处理点击、拖拽、悬停和手牌布局动画。
    /// </summary>
    public class CardViewController : MonoBehaviour, IController,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler
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
        bool _isInteractionEnabled = true;
        bool _hasPoseTarget;
        CardDisplayMode _displayMode = CardDisplayMode.Full;

        Tween _moveTween;
        Tween _scaleTween;
        Tween _rotationTween;
        Vector3 _targetLocalPosition;
        Vector3 _targetLocalScale;
        float _targetLocalRotationZ;

        public CardData CardData => _cardData;
        public RectTransform RectTransform => _rectTransform;
        public bool IsDragging => _isDragging;

        public event Action<CardViewController, int> OnCardDroppedToSlot;
        public event Action<CardViewController, int> OnHandReorder;
        public event Action<CardViewController> OnClicked;
        public event Action<CardViewController> OnRightClicked;
        public event Action<CardViewController, bool> OnHoverChanged;
        public event Action<CardViewController> OnDragStarted;
        public event Action<CardViewController, PointerEventData> OnDragging;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnValidate()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_displayView == null)
                _displayView = GetComponent<CardDisplayView>();
        }

        void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Setup(CardData data, CardDisplayMode mode = CardDisplayMode.Full)
        {
            _cardData = data;
            _displayMode = mode;
            _isInteractionEnabled = true;
            _hasPoseTarget = false;
            GetDisplayView().Setup(data, _displayMode);
        }

        public void SetDisplayMode(CardDisplayMode mode)
        {
            if (_cardData == null || _displayMode == mode)
                return;

            _displayMode = mode;
            GetDisplayView().Setup(_cardData, _displayMode);
        }

        public void SetRedrawSelected(bool selected)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.SetActive(selected);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isInteractionEnabled)
                return;

            StopPoseTweens();

            _isDragging = true;
            _hasInvokedDrop = false;
            _originalParent = transform.parent;
            _originalPosition = transform.localPosition;
            _originalSiblingIndex = transform.GetSiblingIndex();

            SetRaycastBlock(false);
            UILayerManager.MoveToLayer(transform, UILayer.Drag, true);
            StraightenForDrag();
            OnDragStarted?.Invoke(this);
            MoveToPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            MoveToPointer(eventData);
            OnDragging?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isInteractionEnabled)
                return;

            _isDragging = false;
            SetRaycastBlock(true);

            if (_hasInvokedDrop)
                return;

            int slotIndex = FindSlotUnderPointer(eventData);
            if (slotIndex >= 0)
            {
                _hasInvokedDrop = true;
                OnCardDroppedToSlot?.Invoke(this, slotIndex);
                return;
            }

            int newSiblingIndex = ComputeSiblingIndexFromX(eventData);
            if (OnHandReorder != null)
                OnHandReorder.Invoke(this, newSiblingIndex);
            else
                ReturnToHandFromCurrentPosition();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging || !_isInteractionEnabled)
                return;

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClicked?.Invoke(this);
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
                OnClicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isDragging || !_isInteractionEnabled)
                return;

            OnHoverChanged?.Invoke(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isDragging || !_isInteractionEnabled)
                return;

            OnHoverChanged?.Invoke(this, false);
        }

        public void ReturnToHand()
        {
            ResetDragState();
            RestoreOriginalParent(true);
        }

        public void ReturnToHandFromCurrentPosition()
        {
            ResetDragState();
            RestoreOriginalParent(true, false);
        }

        public void ResetDragState()
        {
            _isDragging = false;
            _hasInvokedDrop = false;
            _isInteractionEnabled = true;
            _hasPoseTarget = false;
            SetRaycastBlock(true);
            SetRedrawSelected(false);
            StopPoseTweens();
        }

        public void SetInteractionEnabled(bool enabled)
        {
            _isInteractionEnabled = enabled;
            SetRaycastBlock(enabled);
        }

        public void SetLocalPose(Vector3 localPosition, Vector3 localScale, float localRotationZ)
        {
            StopPoseTweens();
            _rectTransform.localPosition = localPosition;
            _rectTransform.localScale = localScale;
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, localRotationZ);
            CachePoseTarget(localPosition, localScale, localRotationZ);
        }

        public void AnimateToLocalPose(
            Vector3 localPosition,
            Vector3 localScale,
            float localRotationZ,
            bool instant,
            float duration = 0.18f)
        {
            if (_rectTransform == null)
                return;

            if (instant)
            {
                SetLocalPose(localPosition, localScale, localRotationZ);
                return;
            }

            if (HasSamePoseTarget(localPosition, localScale, localRotationZ))
                return;

            StopPoseTweens();
            if (!Approximately(_rectTransform.localPosition, localPosition))
                _moveTween = Tween.LocalPosition(_rectTransform, localPosition, duration, Ease.OutCubic);
            if (!Approximately(_rectTransform.localScale, localScale))
                _scaleTween = Tween.Scale(_rectTransform, localScale, duration, Ease.OutCubic);

            float currentRotationZ = NormalizeAngle(_rectTransform.localEulerAngles.z);
            if (!ApproximatelyAngle(currentRotationZ, localRotationZ))
            {
                _rotationTween = Tween.LocalEulerAngles(
                    _rectTransform,
                    new Vector3(0f, 0f, currentRotationZ),
                    new Vector3(0f, 0f, localRotationZ),
                    duration,
                    Ease.OutCubic);
            }

            CachePoseTarget(localPosition, localScale, localRotationZ);
        }

        void MoveToPointer(PointerEventData eventData)
        {
            var parentRect = _rectTransform.parent as RectTransform;
            if (parentRect == null)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);

            _rectTransform.localPosition = localPoint;
        }

        int FindSlotUnderPointer(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponentInParent<CardViewController>() != null)
                    continue;

                CardSlotView slot = result.gameObject.GetComponentInParent<CardSlotView>();
                if (slot != null)
                    return slot.SlotIndex;
            }

            return -1;
        }

        int ComputeSiblingIndexFromX(PointerEventData eventData)
        {
            var parentRect = _originalParent as RectTransform;
            if (parentRect == null)
                return _originalSiblingIndex;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPos);

            int insertPos = 0;
            for (int i = 0; i < _originalParent.childCount; i++)
            {
                Transform child = _originalParent.GetChild(i);
                if (child == transform)
                    continue;

                if (child.localPosition.x < localPos.x)
                    insertPos++;
            }

            return insertPos;
        }

        void RestoreOriginalParent(bool worldPositionStays, bool restoreLocalPosition = true)
        {
            if (_originalParent == null)
                return;

            StopPoseTweens();
            transform.SetParent(_originalParent, worldPositionStays);
            transform.SetSiblingIndex(_originalSiblingIndex);
            if (restoreLocalPosition)
                transform.localPosition = _originalPosition;
        }

        void SetRaycastBlock(bool blocksRaycasts)
        {
            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = blocksRaycasts;
        }

        void StopPoseTweens()
        {
            _moveTween.Stop();
            _scaleTween.Stop();
            _rotationTween.Stop();
        }

        void StraightenForDrag()
        {
            _rectTransform.localRotation = Quaternion.identity;
            _hasPoseTarget = false;
        }

        void CachePoseTarget(Vector3 localPosition, Vector3 localScale, float localRotationZ)
        {
            _targetLocalPosition = localPosition;
            _targetLocalScale = localScale;
            _targetLocalRotationZ = NormalizeAngle(localRotationZ);
            _hasPoseTarget = true;
        }

        bool HasSamePoseTarget(Vector3 localPosition, Vector3 localScale, float localRotationZ)
        {
            if (!_hasPoseTarget)
                return false;

            return Approximately(_targetLocalPosition, localPosition)
                && Approximately(_targetLocalScale, localScale)
                && ApproximatelyAngle(_targetLocalRotationZ, localRotationZ);
        }

        static bool Approximately(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude <= 0.0001f;
        }

        static bool ApproximatelyAngle(float a, float b)
        {
            return Mathf.Abs(Mathf.DeltaAngle(a, b)) <= 0.01f;
        }

        static float NormalizeAngle(float angle)
        {
            return Mathf.Repeat(angle + 180f, 360f) - 180f;
        }

        CardDisplayView GetDisplayView()
        {
            if (_displayView == null)
            {
                _displayView = GetComponent<CardDisplayView>();
                if (_displayView == null)
                    _displayView = gameObject.AddComponent<CardDisplayView>();
            }

            return _displayView;
        }
    }
}
