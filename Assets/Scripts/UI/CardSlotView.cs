using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 出牌槽视图：显示槽位编号和当前放置的卡牌，处理点击取消出牌。
    /// </summary>
    public class CardSlotView : MonoBehaviour, IController
    {
        [SerializeField, Range(0, 4)] int _slotIndex;
        [SerializeField] TMPro.TextMeshProUGUI _slotLabel;
        [SerializeField] TMPro.TextMeshProUGUI _cardNameText;
        [SerializeField] Image _cardArtwork;
        [SerializeField] Image _slotBackground;
        [SerializeField] GameObject _emptyIndicator;
        [SerializeField] GameObject _filledIndicator;

        [ShowInInspector, ReadOnly] CardData _currentCard;

        public int SlotIndex => _slotIndex;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Awake()
        {
            if (_slotLabel != null)
                _slotLabel.text = $"槽 {_slotIndex + 1}";
        }

        void OnEnable()
        {
            this.RegisterEvent<CardPlayedEvent>(OnCardPlayed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<CardRemovedFromSlotEvent>(OnCardRemoved).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SlotEffectsResolvedEvent>(OnSlotsResolved).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleStartedEvent>(OnBattleStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnBattleStarted(BattleStartedEvent e) => ClearSlot();

        void OnCardPlayed(CardPlayedEvent e)
        {
            if (e.SlotIndex != _slotIndex) return;

            var battleModel = this.GetModel<BattleModel>();
            _currentCard = battleModel.PlaySlots[_slotIndex];
            RefreshUI();
        }

        void OnCardRemoved(CardRemovedFromSlotEvent e)
        {
            if (e.SlotIndex != _slotIndex) return;
            ClearSlot();
        }

        void OnSlotsResolved(SlotEffectsResolvedEvent e) => ClearSlot();

        public void ClearSlot()
        {
            _currentCard = null;
            RefreshUI();
        }

        void RefreshUI()
        {
            bool filled = _currentCard != null;

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

        public void OnClickReturnCard()
        {
            if (_currentCard == null) return;
            this.SendCommand(new ReturnCardToHandCommand(_slotIndex));
        }
    }
}
