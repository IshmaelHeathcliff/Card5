using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 牌堆弹窗中单条卡牌记录：显示卡牌名称、费用和效果描述。
    /// </summary>
    public class CardListEntryView : MonoBehaviour
    {
        [SerializeField] CardDisplayView _displayView;

        void OnValidate()
        {
            if (_displayView == null)
                _displayView = GetComponent<CardDisplayView>();
        }

        public void Setup(CardData card)
        {
            GetDisplayView().Setup(card);
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
