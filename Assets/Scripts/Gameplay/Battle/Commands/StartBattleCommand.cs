namespace Card5
{
    public class StartBattleCommand : AbstractCommand
    {
        readonly DeckPresetData _deckPreset;
        readonly MonsterListData _monsterList;
        readonly EnemyData _enemyData;
        readonly BattleRewardConfigData _rewardConfig;
        readonly int _playerMaxHp;
        readonly int _maxEnergy;

        public StartBattleCommand(
            DeckPresetData deckPreset,
            MonsterListData monsterList,
            EnemyData enemyData,
            BattleRewardConfigData rewardConfig,
            int playerMaxHp = 30,
            int maxEnergy = 3)
        {
            _deckPreset = deckPreset;
            _monsterList = monsterList;
            _enemyData = enemyData;
            _rewardConfig = rewardConfig;
            _playerMaxHp = playerMaxHp;
            _maxEnergy = maxEnergy;
        }

        public StartBattleCommand(
            DeckPresetData deckPreset,
            EnemyData enemyData,
            BattleRewardConfigData rewardConfig,
            int playerMaxHp = 30,
            int maxEnergy = 3)
            : this(deckPreset, null, enemyData, rewardConfig, playerMaxHp, maxEnergy)
        {
        }

        public StartBattleCommand(DeckPresetData deckPreset, EnemyData enemyData, int playerMaxHp = 30, int maxEnergy = 3)
            : this(deckPreset, enemyData, null, playerMaxHp, maxEnergy)
        {
        }

        protected override void OnExecute()
        {
            this.GetSystem<BattleSystem>().StartBattle(_deckPreset, _monsterList, _enemyData, _rewardConfig, _playerMaxHp, _maxEnergy);
        }
    }
}
