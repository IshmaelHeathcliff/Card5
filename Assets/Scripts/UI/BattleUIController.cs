using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
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

        [Title("按钮")]
        [SerializeField] Button _endTurnButton;
        [SerializeField] Button _redrawButton;
        [SerializeField] TMPro.TextMeshProUGUI _redrawCountText;

        [Title("手牌控制器引用")]
        [SerializeField, Required] HandViewController _handViewController;

        [Title("战斗结果面板")]
        [SerializeField] GameObject _winPanel;
        [SerializeField] GameObject _losePanel;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnEnable()
        {
            this.RegisterEvent<BattleStartedEvent>(OnBattleStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<PlayerHpChangedEvent>(OnPlayerHpChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<EnergyChangedEvent>(OnEnergyChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<TurnStartedEvent>(OnTurnStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleEndedEvent>(OnBattleEnded).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<RedrawCountChangedEvent>(OnRedrawCountChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleRewardOfferedEvent>(OnBattleRewardOffered).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void Start()
        {
            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);

            if (_redrawButton != null)
                _redrawButton.onClick.AddListener(OnRedrawClicked);

            if (_winPanel != null) _winPanel.SetActive(false);
            if (_losePanel != null) _losePanel.SetActive(false);
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

        void OnBattleEnded(BattleEndedEvent e)
        {
            if (_endTurnButton != null) _endTurnButton.interactable = false;
            if (_redrawButton != null) _redrawButton.interactable = false;

            if (e.PlayerWon)
            {
                if (_winPanel != null) _winPanel.SetActive(true);
            }
            else
            {
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
    }
}
