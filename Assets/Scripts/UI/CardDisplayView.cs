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
        [SerializeField] Transform _name;
        [SerializeField] Transform _cost;
        [SerializeField] Transform _description;

        public void Setup(CardData card, CardDisplayMode mode = CardDisplayMode.Full)
        {
            if (card == null)
            {
                Clear();
                return;
            }

            bool showName = mode != CardDisplayMode.Compact;
            bool showCost = mode == CardDisplayMode.Full;
            bool showDescription = mode == CardDisplayMode.Full;

            SetNodeActive(_name, showName);
            SetNodeActive(_cost, showCost);
            SetNodeActive(_description, showDescription);

            if (_nameText != null)
                _nameText.text = showName ? card.CardName : string.Empty;

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
            SetNodeActive(_name, false);
            SetNodeActive(_cost, false);
            SetNodeActive(_description, false);

            if (_nameText != null)
                _nameText.text = string.Empty;
            if (_costText != null)
                _costText.text = string.Empty;
            if (_descriptionText != null)
                _descriptionText.text = string.Empty;
            if (_artworkImage != null)
                _artworkImage.enabled = false;
        }

        static void SetNodeActive(Transform node, bool active)
        {
            if (node != null && node.gameObject.activeSelf != active)
                node.gameObject.SetActive(active);
        }
    }
}
