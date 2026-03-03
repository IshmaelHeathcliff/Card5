namespace Card5
{
    public class StartBattleCommand : AbstractCommand
    {
        readonly DeckPresetData _deckPreset;
        readonly EnemyData _enemyData;
        readonly int _playerMaxHp;
        readonly int _maxEnergy;

        public StartBattleCommand(DeckPresetData deckPreset, EnemyData enemyData, int playerMaxHp = 30, int maxEnergy = 3)
        {
            _deckPreset = deckPreset;
            _enemyData = enemyData;
            _playerMaxHp = playerMaxHp;
            _maxEnergy = maxEnergy;
        }

        protected override void OnExecute()
        {
            this.GetSystem<BattleSystem>().StartBattle(_deckPreset, _enemyData, _playerMaxHp, _maxEnergy);
        }
    }
}
