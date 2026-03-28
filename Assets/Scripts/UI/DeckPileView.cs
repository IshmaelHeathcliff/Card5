using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 抽牌堆 / 弃牌堆按钮视图：显示当前张数，点击弹出卡牌列表。
    /// 在 Inspector 中通过 _isDrawPile 切换两种用途。
    /// </summary>
    public class DeckPileView : MonoBehaviour, IController
    {
        [SerializeField] TMPro.TextMeshProUGUI _countText;
        [SerializeField] Button _button;

        [SerializeField, LabelText("是否为抽牌堆")]
        bool _isDrawPile = true;

        [SerializeField, LabelText("显示名称")]
        string _pileName = "牌堆";

        [SerializeField, Required]
        CardListPopupView _popup;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Start()
        {
            _button?.onClick.AddListener(OnClick);
        }

        void OnEnable()
        {
            if (_isDrawPile)
                this.RegisterEvent<DrawPileChangedEvent>(e => SetCount(e.Count))
                    .UnRegisterWhenGameObjectDestroyed(gameObject);
            else
                this.RegisterEvent<DiscardPileChangedEvent>(e => SetCount(e.Count))
                    .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<BattleStartedEvent>(OnBattleStarted)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnDestroy()
        {
            _button?.onClick.RemoveListener(OnClick);
        }

        void OnBattleStarted(BattleStartedEvent e)
        {
            var deck = this.GetModel<DeckModel>();
            SetCount(_isDrawPile ? deck.DrawPile.Count : deck.DiscardPile.Count);
        }

        void SetCount(int count)
        {
            if (_countText != null)
                _countText.text = count.ToString();
        }

        void OnClick()
        {
            if (_popup == null) return;

            var deck = this.GetModel<DeckModel>();
            var cards = _isDrawPile
                ? (System.Collections.Generic.IReadOnlyList<CardData>)deck.DrawPile
                : deck.DiscardPile;

            _popup.Show(_pileName, cards);
        }
    }
}
