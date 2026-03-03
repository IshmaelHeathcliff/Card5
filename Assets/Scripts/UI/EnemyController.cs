using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 敌人 Controller：持有运行时 HP，响应战斗事件。
    /// 目前为无行为的木桩，预留 IEnemyBehavior 扩展接口。
    /// </summary>
    public class EnemyController : MonoBehaviour, IController
    {
        [SerializeField] Image _portrait;
        [SerializeField] Slider _hpSlider;
        [SerializeField] TMPro.TextMeshProUGUI _hpText;
        [SerializeField] TMPro.TextMeshProUGUI _nameText;

        [ShowInInspector, ReadOnly] int _currentHp;
        [ShowInInspector, ReadOnly] int _maxHp;

        EnemyData _data;

        /// <summary>后续挂载状态机/行为树实现此接口</summary>
        IEnemyBehavior _behavior;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        public void SetBehavior(IEnemyBehavior behavior) => _behavior = behavior;

        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;
        public bool IsAlive => _currentHp > 0;

        void Awake()
        {
            this.GetSystem<BattleSystem>().RegisterEnemy(this);
        }

        void OnEnable()
        {
            this.RegisterEvent<BattleStartedEvent>(OnBattleStarted).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnBattleStarted(BattleStartedEvent e)
        {
            RefreshUI();
        }

        public void InitEnemy(EnemyData data)
        {
            _data = data;
            _currentHp = data.MaxHp;
            _maxHp = data.MaxHp;

            if (_portrait != null && data.Portrait != null)
                _portrait.sprite = data.Portrait;
            if (_nameText != null)
                _nameText.text = data.EnemyName;

            RefreshUI();
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            _currentHp -= amount;
            if (_currentHp < 0) _currentHp = 0;

            this.GetSystem<BattleSystem>().NotifyEnemyDamaged(amount, _currentHp, _maxHp);

            RefreshUI();
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;

            _currentHp += amount;
            if (_currentHp > _maxHp) _currentHp = _maxHp;

            this.GetSystem<BattleSystem>().NotifyEnemyHealed(amount, _currentHp, _maxHp);

            RefreshUI();
        }

        void RefreshUI()
        {
            if (_hpSlider != null)
            {
                _hpSlider.maxValue = _maxHp;
                _hpSlider.value = _currentHp;
            }

            if (_hpText != null)
                _hpText.text = $"{_currentHp} / {_maxHp}";
        }
    }
}
