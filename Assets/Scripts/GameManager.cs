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
        [SerializeField, Required] DeckPresetData _startingDeck;
        [SerializeField, Required] EnemyData _enemyData;

        [SerializeField] int _playerMaxHp = 30;
        [SerializeField] int _maxEnergy = 3;

        [SerializeField] int _targetFrameRate = 60;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;
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
            if (_startingDeck == null || _enemyData == null)
            {
                Debug.LogWarning("[GameManager] 请在 Inspector 中设置 StartingDeck 和 EnemyData");
                return;
            }

            this.SendCommand(new StartBattleCommand(_startingDeck, _enemyData, _playerMaxHp, _maxEnergy));
        }
    }
}
