using Card5.Gameplay.Events;
using Cysharp.Threading.Tasks;
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

        public static DeckPileView DrawPileInstance { get; private set; }
        public static DeckPileView DiscardPileInstance { get; private set; }

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Start()
        {
            _button?.onClick.AddListener(OnClick);
        }

        void OnEnable()
        {
            if (_isDrawPile)
                DrawPileInstance = this;
            else
                DiscardPileInstance = this;

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
            if (DrawPileInstance == this)
                DrawPileInstance = null;
            if (DiscardPileInstance == this)
                DiscardPileInstance = null;

            _button?.onClick.RemoveListener(OnClick);
        }

        public Vector3 GetAnchorWorldPosition()
        {
            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
                return rectTransform.TransformPoint(rectTransform.rect.center);

            return transform.position;
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
            if (UIPopupManager.Instance == null)
            {
                Debug.LogWarning("[DeckPileView] UIPopupManager 未就绪，无法打开牌堆弹窗");
                return;
            }

            var deck = this.GetModel<DeckModel>();
            var cards = _isDrawPile
                ? (System.Collections.Generic.IReadOnlyList<CardData>)deck.DrawPile
                : deck.DiscardPile;

            UIPopupManager.Instance.ShowCardListAsync(_pileName, cards).Forget();
        }
    }
}
