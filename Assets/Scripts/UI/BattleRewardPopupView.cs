using System.Collections.Generic;
using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    public class BattleRewardPopupView : MonoBehaviour, IController
    {
        [Title("面板")]
        [SerializeField] GameObject _root;
        [SerializeField] TMPro.TextMeshProUGUI _titleText;

        [Title("选项")]
        [SerializeField, Required] Transform _contentContainer;
        [SerializeField, Required] BattleRewardOptionView _optionPrefab;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnValidate()
        {
            if (_root == null)
                _root = transform.Find("Root")?.gameObject;

            if (_contentContainer == null)
                _contentContainer = transform.Find("Content");
        }

        void Awake()
        {
            Hide();
        }

        void OnEnable()
        {
            this.RegisterEvent<BattleRewardOfferedEvent>(OnRewardOffered).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleRewardOptionClaimedEvent>(OnRewardOptionClaimed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleRewardCompletedEvent>(_ => Hide()).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleEndedEvent>(_ => Hide()).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnRewardOffered(BattleRewardOfferedEvent e)
        {
            Show(e.Offers);
        }

        void OnRewardOptionClaimed(BattleRewardOptionClaimedEvent e)
        {
            if (e.RemainingOffers.Count > 0)
                Show(e.RemainingOffers);
        }

        void Show(IReadOnlyList<BattleRewardOffer> offers)
        {
            ClearOptions();

            if (_titleText != null)
                _titleText.text = "选择奖励";

            foreach (BattleRewardOffer offer in offers)
            {
                foreach (BattleRewardOption option in offer.Options)
                    SpawnOption(offer, option);
            }

            if (_root != null)
                _root.SetActive(true);
        }

        void SpawnOption(BattleRewardOffer offer, BattleRewardOption option)
        {
            if (_optionPrefab == null || _contentContainer == null) return;

            BattleRewardOptionView optionView = Instantiate(_optionPrefab, _contentContainer);
            optionView.Setup(offer, option);
            optionView.Clicked += OnOptionClicked;
        }

        void OnOptionClicked(BattleRewardOffer offer, BattleRewardOption option)
        {
            this.SendCommand(new SelectBattleRewardCommand(offer.OfferId, option.OptionId));
        }

        void Hide()
        {
            ClearOptions();

            if (_root != null)
                _root.SetActive(false);
        }

        void ClearOptions()
        {
            if (_contentContainer == null) return;

            foreach (Transform child in _contentContainer)
            {
                BattleRewardOptionView optionView = child.GetComponent<BattleRewardOptionView>();
                if (optionView != null)
                    optionView.Clicked -= OnOptionClicked;

                Destroy(child.gameObject);
            }
        }
    }
}
