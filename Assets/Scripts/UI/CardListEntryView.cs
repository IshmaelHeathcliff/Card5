using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 牌堆弹窗中单条卡牌记录：显示卡牌名称、费用和效果描述。
    /// </summary>
    public class CardListEntryView : MonoBehaviour
    {
        [SerializeField] TMPro.TextMeshProUGUI _nameText;
        [SerializeField] TMPro.TextMeshProUGUI _costText;
        [SerializeField] TMPro.TextMeshProUGUI _descriptionText;
        [SerializeField] Image _artworkImage;

        public void Setup(CardData card)
        {
            if (_nameText != null) _nameText.text = card.CardName;
            if (_costText != null) _costText.text = card.EnergyCost.ToString();

            if (_descriptionText != null)
            {
                var sb = new StringBuilder();
                foreach (var effect in card.Effects)
                    sb.AppendLine(effect.GetDescription());
                _descriptionText.text = sb.ToString().TrimEnd();
            }

            if (_artworkImage != null && card.Artwork != null)
                _artworkImage.sprite = card.Artwork;
        }
    }
}
