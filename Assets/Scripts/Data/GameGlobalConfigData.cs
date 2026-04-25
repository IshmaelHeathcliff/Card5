using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "GameGlobalConfig", menuName = "Card5/Game Global Config")]
    public class GameGlobalConfigData : ScriptableObject
    {
        [Title("战斗入口")]
        [SerializeField, LabelText("初始牌组"), Required] DeckPresetData _startingDeck;
        [SerializeField, LabelText("怪物列表")] MonsterListData _monsterList;
        [SerializeField, LabelText("兼容敌人配置"), Required] EnemyData _enemyData;
        [SerializeField, LabelText("奖励配置")] BattleRewardConfigData _rewardConfig;

        [Title("玩家初始数值")]
        [SerializeField, LabelText("玩家最大生命值"), MinValue(1)] int _playerMaxHp = 30;
        [SerializeField, LabelText("最大能量"), MinValue(0)] int _maxEnergy = 3;

        [Title("运行设置")]
        [SerializeField, LabelText("目标帧率"), MinValue(-1)] int _targetFrameRate = 60;

        public DeckPresetData StartingDeck => _startingDeck;
        public MonsterListData MonsterList => _monsterList;
        public EnemyData EnemyData => _enemyData;
        public BattleRewardConfigData RewardConfig => _rewardConfig;
        public int PlayerMaxHp => _playerMaxHp;
        public int MaxEnergy => _maxEnergy;
        public int TargetFrameRate => _targetFrameRate;
    }
}
