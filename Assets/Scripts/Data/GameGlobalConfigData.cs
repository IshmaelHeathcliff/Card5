using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "GameGlobalConfig", menuName = "Card5/Game Global Config")]
    public class GameGlobalConfigData : ScriptableObject
    {
        [Title("战斗入口")]
        [SerializeField, Required] DeckPresetData _startingDeck;
        [SerializeField, Required] EnemyData _enemyData;
        [SerializeField] BattleRewardConfigData _rewardConfig;

        [Title("玩家初始数值")]
        [SerializeField, MinValue(1)] int _playerMaxHp = 30;
        [SerializeField, MinValue(0)] int _maxEnergy = 3;

        [Title("运行设置")]
        [SerializeField, MinValue(-1)] int _targetFrameRate = 60;

        public DeckPresetData StartingDeck => _startingDeck;
        public EnemyData EnemyData => _enemyData;
        public BattleRewardConfigData RewardConfig => _rewardConfig;
        public int PlayerMaxHp => _playerMaxHp;
        public int MaxEnergy => _maxEnergy;
        public int TargetFrameRate => _targetFrameRate;
    }
}
