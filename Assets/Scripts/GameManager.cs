using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Card5
{
    /// <summary>
    /// 游戏入口 MonoBehaviour，挂载在场景中的 GameManager GameObject 上。
    /// 负责持有战斗配置引用并触发战斗启动。
    /// </summary>
    public class GameManager : MonoBehaviour, IController
    {
        [SerializeField] GameGlobalConfigData _globalConfig;

        [SerializeField, Required] DeckPresetData _startingDeck;
        [SerializeField, Required] EnemyData _enemyData;
        [SerializeField] BattleRewardConfigData _rewardConfig;

        [SerializeField] int _playerMaxHp = 30;
        [SerializeField] int _maxEnergy = 3;

        [SerializeField] int _targetFrameRate = 60;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        DeckPresetData StartingDeck => _globalConfig != null && _globalConfig.StartingDeck != null
            ? _globalConfig.StartingDeck
            : _startingDeck;

        EnemyData EnemyData => _globalConfig != null && _globalConfig.EnemyData != null
            ? _globalConfig.EnemyData
            : _enemyData;

        BattleRewardConfigData RewardConfig => _globalConfig != null && _globalConfig.RewardConfig != null
            ? _globalConfig.RewardConfig
            : _rewardConfig;

        int PlayerMaxHp => _globalConfig != null ? _globalConfig.PlayerMaxHp : _playerMaxHp;
        int MaxEnergy => _globalConfig != null ? _globalConfig.MaxEnergy : _maxEnergy;
        int TargetFrameRate => _globalConfig != null ? _globalConfig.TargetFrameRate : _targetFrameRate;

        void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
        }

        async UniTaskVoid Start()
        {
            await WaitForPoolAsync();
            StartBattle();
        }

        async UniTask WaitForPoolAsync()
        {
            while (CardViewPool.Instance == null || !CardViewPool.Instance.IsReady)
                await UniTask.Yield();
        }

        [Button("开始战斗")]
        public void StartBattle()
        {
            DeckPresetData startingDeck = StartingDeck;
            EnemyData enemyData = EnemyData;

            if (startingDeck == null || enemyData == null)
            {
                Debug.LogWarning("[GameManager] 请在 Inspector 中设置 StartingDeck 和 EnemyData");
                return;
            }

            this.SendCommand(new StartBattleCommand(startingDeck, enemyData, RewardConfig, PlayerMaxHp, MaxEnergy));
        }
    }
}
