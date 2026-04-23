using System;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    public class BattleRewardOptionView : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] CardDisplayView _displayView;

        BattleRewardOffer _offer;
        BattleRewardOption _option;

        public event Action<BattleRewardOffer, BattleRewardOption> Clicked;

        void OnValidate()
        {
            if (_button == null)
                _button = GetComponent<Button>();
            if (_displayView == null)
                _displayView = GetComponent<CardDisplayView>();
        }

        void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();
            GetDisplayView();
        }

        void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClicked);
        }

        void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }

        public void Setup(BattleRewardOffer offer, BattleRewardOption option)
        {
            _offer = offer;
            _option = option;

            if (option.RewardType == BattleRewardType.Card)
                SetupCard(option.Card);
        }

        void SetupCard(CardData card)
        {
            GetDisplayView().Setup(card);
        }

        void OnClicked()
        {
            if (_offer == null || _option == null) return;
            Clicked?.Invoke(_offer, _option);
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
