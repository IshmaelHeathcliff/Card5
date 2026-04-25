using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 战斗 UI 控制器：显示能量、回合数，处理「结束回合」和「重抽」按钮。
    /// </summary>
    public class BattleUIController : MonoBehaviour, IController
    {
        [Title("能量")]
        [SerializeField] TMPro.TextMeshProUGUI _energyText;

        [Title("回合")]
        [SerializeField] TMPro.TextMeshProUGUI _turnText;
        [SerializeField] TMPro.TextMeshProUGUI _monsterPlayRoundsText;

        [Title("按钮")]
        [SerializeField] Button _endTurnButton;
        [SerializeField] Button _redrawButton;
        [SerializeField] TMPro.TextMeshProUGUI _redrawCountText;

        [Title("手牌控制器引用")]
        [SerializeField, Required] HandViewController _handViewController;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnEnable()
        {
            this.RegisterEvent<BattleStartedEvent>(OnBattleStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<EnergyChangedEvent>(OnEnergyChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<TurnStartedEvent>(OnTurnStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<MonsterPlayRoundCountChangedEvent>(OnMonsterPlayRoundCountChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleEndedEvent>(OnBattleEnded).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<RedrawCountChangedEvent>(OnRedrawCountChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleRewardOfferedEvent>(OnBattleRewardOffered).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void Start()
        {
            EnsureMonsterPlayRoundsText();

            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);

            if (_redrawButton != null)
                _redrawButton.onClick.AddListener(OnRedrawClicked);
        }

        void OnDestroy()
        {
            if (_endTurnButton != null)
                _endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            if (_redrawButton != null)
                _redrawButton.onClick.RemoveListener(OnRedrawClicked);
        }

        void OnBattleStarted(BattleStartedEvent e)
        {
            if (_endTurnButton != null) _endTurnButton.interactable = true;
            if (_redrawButton != null) _redrawButton.interactable = true;
            UIPopupManager.Instance?.HideAll();
        }

        void OnEnergyChanged(EnergyChangedEvent e)
        {
            if (_energyText != null)
                _energyText.text = $"能量: {e.CurrentEnergy} / {e.MaxEnergy}";
        }

        void OnTurnStarted(TurnStartedEvent e)
        {
            if (_turnText != null)
                _turnText.text = $"第 {e.TurnNumber} 回合";

            if (_endTurnButton != null) _endTurnButton.interactable = true;
        }

        void OnMonsterPlayRoundCountChanged(MonsterPlayRoundCountChangedEvent e)
        {
            if (_monsterPlayRoundsText == null) return;
            int remaining = Mathf.Max(0, e.MaxCount - e.CurrentRound);
            _monsterPlayRoundsText.text = $"剩余出牌: {remaining}/{e.MaxCount}";
        }

        void OnBattleEnded(BattleEndedEvent e)
        {
            if (_endTurnButton != null) _endTurnButton.interactable = false;
            if (_redrawButton != null) _redrawButton.interactable = false;
        }

        void OnBattleRewardOffered(BattleRewardOfferedEvent e)
        {
            if (_endTurnButton != null) _endTurnButton.interactable = false;
            if (_redrawButton != null) _redrawButton.interactable = false;
        }

        void OnRedrawCountChanged(RedrawCountChangedEvent e)
        {
            if (_redrawCountText != null)
                _redrawCountText.text = $"重抽: {e.Remaining}/{e.Max}";

            if (_redrawButton != null)
                _redrawButton.interactable = e.Remaining > 0;
        }

        void OnEndTurnClicked()
        {
            if (_endTurnButton != null) _endTurnButton.interactable = false;
            this.SendCommand<EndTurnCommand>();
        }

        void OnRedrawClicked()
        {
            if (_handViewController != null)
                _handViewController.EnterRedrawMode();
        }

        void EnsureMonsterPlayRoundsText()
        {
            if (_monsterPlayRoundsText != null) return;
            if (_turnText == null) return;

            Transform parent = _turnText.transform.parent;
            var textObject = new GameObject("MonsterPlayRoundsText", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            var sourceRect = (RectTransform)_turnText.transform;
            var rectTransform = (RectTransform)textObject.transform;
            rectTransform.anchorMin = sourceRect.anchorMin;
            rectTransform.anchorMax = sourceRect.anchorMax;
            rectTransform.pivot = sourceRect.pivot;
            rectTransform.sizeDelta = sourceRect.sizeDelta;
            rectTransform.anchoredPosition = sourceRect.anchoredPosition + new Vector2(0f, -32f);

            _monsterPlayRoundsText = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            _monsterPlayRoundsText.font = _turnText.font;
            _monsterPlayRoundsText.fontSharedMaterial = _turnText.fontSharedMaterial;
            _monsterPlayRoundsText.fontSize = _turnText.fontSize;
            _monsterPlayRoundsText.alignment = _turnText.alignment;
            _monsterPlayRoundsText.color = _turnText.color;
            _monsterPlayRoundsText.raycastTarget = false;
            _monsterPlayRoundsText.text = "剩余出牌: -";
        }

    }
}
