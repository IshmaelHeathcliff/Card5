using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    public enum CardDisplayMode
    {
        Full,
        Compact,
        Slot
    }

    public class CardDisplayView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _nameText;
        [SerializeField] TextMeshProUGUI _costText;
        [SerializeField] TextMeshProUGUI _descriptionText;
        [SerializeField] Image _artworkImage;

        public void Bind(
            TextMeshProUGUI nameText,
            TextMeshProUGUI costText,
            TextMeshProUGUI descriptionText,
            Image artworkImage)
        {
            _nameText = nameText;
            _costText = costText;
            _descriptionText = descriptionText;
            _artworkImage = artworkImage;
        }

        public void Setup(CardData card, CardDisplayMode mode = CardDisplayMode.Full)
        {
            if (card == null)
            {
                Clear();
                return;
            }

            bool showCost = mode != CardDisplayMode.Slot;
            bool showDescription = mode == CardDisplayMode.Full;

            if (_nameText != null)
                _nameText.text = card.CardName;

            if (_costText != null)
                _costText.text = showCost ? card.EnergyCost.ToString() : string.Empty;

            if (_descriptionText != null)
                _descriptionText.text = showDescription ? card.GetFullDescription() : string.Empty;

            if (_artworkImage != null)
            {
                _artworkImage.enabled = card.Artwork != null;
                if (card.Artwork != null)
                    _artworkImage.sprite = card.Artwork;
            }
        }

        public void Clear()
        {
            if (_nameText != null)
                _nameText.text = string.Empty;
            if (_costText != null)
                _costText.text = string.Empty;
            if (_descriptionText != null)
                _descriptionText.text = string.Empty;
            if (_artworkImage != null)
                _artworkImage.enabled = false;
        }
    }
}
