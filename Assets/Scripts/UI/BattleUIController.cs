using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 战斗 UI 控制器：显示玩家 HP、能量、回合数，处理「结束回合」和「重抽」按钮。
    /// </summary>
    public class BattleUIController : MonoBehaviour, IController
    {
        [Title("玩家状态")]
        [SerializeField] Slider _playerHpSlider;
        [SerializeField] TMPro.TextMeshProUGUI _playerHpText;

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

        [Title("战斗结果面板")]
        [SerializeField] GameObject _winPanel;
        [SerializeField] Button _winConfirmButton;
        [SerializeField] GameObject _losePanel;
        [SerializeField] Button _loseConfirmButton;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnValidate()
        {
            if (_winPanel != null && _winConfirmButton == null)
                _winConfirmButton = _winPanel.GetComponentInChildren<Button>(true);
            if (_losePanel != null && _loseConfirmButton == null)
                _loseConfirmButton = _losePanel.GetComponentInChildren<Button>(true);
        }

        void OnEnable()
        {
            this.RegisterEvent<BattleStartedEvent>(OnBattleStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<PlayerHpChangedEvent>(OnPlayerHpChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
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
            EnsureWinPanel();
            EnsureLosePanel();

            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);

            if (_redrawButton != null)
                _redrawButton.onClick.AddListener(OnRedrawClicked);

            if (_winConfirmButton != null)
                _winConfirmButton.onClick.AddListener(OnWinConfirmClicked);

            if (_loseConfirmButton != null)
                _loseConfirmButton.onClick.AddListener(OnLoseConfirmClicked);

            if (_winPanel != null) _winPanel.SetActive(false);
            if (_losePanel != null) _losePanel.SetActive(false);
        }

        void OnDestroy()
        {
            if (_endTurnButton != null)
                _endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            if (_redrawButton != null)
                _redrawButton.onClick.RemoveListener(OnRedrawClicked);
            if (_winConfirmButton != null)
                _winConfirmButton.onClick.RemoveListener(OnWinConfirmClicked);
            if (_loseConfirmButton != null)
                _loseConfirmButton.onClick.RemoveListener(OnLoseConfirmClicked);
        }

        void OnBattleStarted(BattleStartedEvent e)
        {
            if (_endTurnButton != null) _endTurnButton.interactable = true;
            if (_redrawButton != null) _redrawButton.interactable = true;
            if (_winPanel != null) _winPanel.SetActive(false);
            if (_losePanel != null) _losePanel.SetActive(false);

            if (_playerHpSlider != null)
            {
                _playerHpSlider.maxValue = e.PlayerMaxHp;
                _playerHpSlider.value = e.PlayerMaxHp;
            }

            if (_playerHpText != null)
                _playerHpText.text = $"{e.PlayerMaxHp} / {e.PlayerMaxHp}";
        }

        void OnPlayerHpChanged(PlayerHpChangedEvent e)
        {
            if (_playerHpSlider != null)
            {
                _playerHpSlider.maxValue = e.MaxHp;
                _playerHpSlider.value = e.CurrentHp;
            }

            if (_playerHpText != null)
                _playerHpText.text = $"{e.CurrentHp} / {e.MaxHp}";
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

            if (e.PlayerWon)
            {
                if (_losePanel != null) _losePanel.SetActive(false);
                if (_winPanel != null) _winPanel.SetActive(true);
            }
            else
            {
                if (_winPanel != null) _winPanel.SetActive(false);
                if (_losePanel != null) _losePanel.SetActive(true);
            }
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

        void OnWinConfirmClicked()
        {
            if (_winPanel != null)
                _winPanel.SetActive(false);
            this.SendCommand<RestartBattleCommand>();
        }

        void OnLoseConfirmClicked()
        {
            if (_losePanel != null)
                _losePanel.SetActive(false);
            this.SendCommand<RestartBattleCommand>();
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

        void EnsureWinPanel()
        {
            if (_winPanel != null)
            {
                if (_winConfirmButton == null)
                    _winConfirmButton = _winPanel.GetComponentInChildren<Button>(true);
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            _winPanel = CreateResultPanel(canvas.transform, "BattleWinPopup", "挑战胜利", "确认后重新开始。", new Color(0.24f, 0.56f, 0.32f, 1f), out _winConfirmButton);
        }

        void EnsureLosePanel()
        {
            if (_losePanel != null)
            {
                if (_loseConfirmButton == null)
                    _loseConfirmButton = _losePanel.GetComponentInChildren<Button>(true);
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            _losePanel = CreateResultPanel(canvas.transform, "BattleLosePopup", "挑战失败", "出牌轮数已耗尽，确认后重新开始。", new Color(0.24f, 0.45f, 0.85f, 1f), out _loseConfirmButton);
        }

        GameObject CreateResultPanel(Transform parent, string panelName, string title, string message, Color buttonColor, out Button confirmButton)
        {
            var panel = new GameObject(panelName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            var rootRect = (RectTransform)panel.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image overlay = panel.GetComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.62f);

            GameObject dialog = new GameObject("Dialog", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            dialog.transform.SetParent(panel.transform, false);

            var dialogRect = (RectTransform)dialog.transform;
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.anchoredPosition = Vector2.zero;
            dialogRect.sizeDelta = new Vector2(420f, 220f);

            Image dialogBackground = dialog.GetComponent<Image>();
            dialogBackground.color = new Color(0.08f, 0.09f, 0.11f, 0.98f);

            VerticalLayoutGroup layout = dialog.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 22, 24);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateResultPanelText(dialog.transform, title, 30, FontStyles.Bold, 44f);
            CreateResultPanelText(dialog.transform, message, 20, FontStyles.Normal, 48f);
            confirmButton = CreateResultConfirmButton(dialog.transform, buttonColor);
            panel.SetActive(false);
            return panel;
        }

        TMPro.TextMeshProUGUI CreateResultPanelText(Transform parent, string text, int fontSize, FontStyles fontStyle, float height)
        {
            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rectTransform = (RectTransform)textObject.transform;
            rectTransform.sizeDelta = new Vector2(360f, height);

            var textComponent = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.alignment = TMPro.TextAlignmentOptions.Center;
            textComponent.color = Color.white;
            textComponent.raycastTarget = false;
            return textComponent;
        }

        Button CreateResultConfirmButton(Transform parent, Color color)
        {
            var buttonObject = new GameObject("ConfirmButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rectTransform = (RectTransform)buttonObject.transform;
            rectTransform.sizeDelta = new Vector2(180f, 46f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.22f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
            button.colors = colors;

            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textComponent = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.text = "确认";
            textComponent.fontSize = 22;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.alignment = TMPro.TextAlignmentOptions.Center;
            textComponent.color = Color.white;
            textComponent.raycastTarget = false;

            return button;
        }
    }
}
