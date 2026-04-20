using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    public class BattleRewardOptionView : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] TMPro.TextMeshProUGUI _nameText;
        [SerializeField] TMPro.TextMeshProUGUI _costText;
        [SerializeField] TMPro.TextMeshProUGUI _descriptionText;
        [SerializeField] Image _artworkImage;

        BattleRewardOffer _offer;
        BattleRewardOption _option;

        public event Action<BattleRewardOffer, BattleRewardOption> Clicked;

        void OnValidate()
        {
            if (_button == null)
                _button = GetComponent<Button>();
        }

        void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();
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
            if (card == null) return;

            if (_nameText != null) _nameText.text = card.CardName;
            if (_costText != null) _costText.text = card.EnergyCost.ToString();

            if (_descriptionText != null)
            {
                var description = new StringBuilder();
                foreach (CardEffectSO effect in card.Effects)
                    description.AppendLine(effect.GetDescription());
                _descriptionText.text = description.ToString().TrimEnd();
            }

            if (_artworkImage != null)
            {
                _artworkImage.enabled = card.Artwork != null;
                if (card.Artwork != null)
                    _artworkImage.sprite = card.Artwork;
            }
        }

        void OnClicked()
        {
            if (_offer == null || _option == null) return;
            Clicked?.Invoke(_offer, _option);
        }
    }
}
