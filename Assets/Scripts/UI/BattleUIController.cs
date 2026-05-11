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
            this.RegisterEvent<CardPlayedEvent>(OnSlotStateChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<CardRemovedFromSlotEvent>(OnSlotStateChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SlotsSwappedEvent>(OnSlotStateChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<HandSlotSwappedEvent>(OnSlotStateChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SlotEffectsResolvedEvent>(OnSlotStateChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
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
            UpdateEndTurnButtonState();
            UpdateRedrawButtonState();
            UIPopupManager.Instance?.HideAll();
        }

        void OnEnergyChanged(EnergyChangedEvent e)
        {
            if (_energyText != null)
                _energyText.text = $"能量: {e.CurrentEnergy} / {e.MaxEnergy}";

            UpdateRedrawButtonState();
        }

        void OnTurnStarted(TurnStartedEvent e)
        {
            if (_turnText != null)
                _turnText.text = $"第 {e.TurnNumber} 回合";

            UpdateEndTurnButtonState();
            UpdateRedrawButtonState();
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

            UpdateRedrawButtonState();
        }

        void OnSlotStateChanged(CardPlayedEvent e) => UpdateEndTurnButtonState();

        void OnSlotStateChanged(CardRemovedFromSlotEvent e) => UpdateEndTurnButtonState();

        void OnSlotStateChanged(SlotsSwappedEvent e) => UpdateEndTurnButtonState();

        void OnSlotStateChanged(HandSlotSwappedEvent e) => UpdateEndTurnButtonState();

        void OnSlotStateChanged(SlotEffectsResolvedEvent e) => UpdateEndTurnButtonState();

        void OnEndTurnClicked()
        {
            if (_endTurnButton != null && !_endTurnButton.interactable)
                return;

            if (_endTurnButton != null) _endTurnButton.interactable = false;
            this.SendCommand<EndTurnCommand>();
        }

        void OnRedrawClicked()
        {
            if (_redrawButton != null && !_redrawButton.interactable)
                return;

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

        void UpdateEndTurnButtonState()
        {
            if (_endTurnButton == null)
                return;

            BattleSystem battleSystem = this.GetSystem<BattleSystem>();
            _endTurnButton.interactable = battleSystem != null && battleSystem.CanEndTurn();
        }

        void UpdateRedrawButtonState()
        {
            if (_redrawButton == null)
                return;

            BattleSystem battleSystem = this.GetSystem<BattleSystem>();
            _redrawButton.interactable = battleSystem != null && battleSystem.CanRedraw();
        }

    }
}
