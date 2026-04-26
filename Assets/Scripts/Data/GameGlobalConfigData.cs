using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "GameGlobalConfig", menuName = "Card5/Game Global Config")]
    public class GameGlobalConfigData : ScriptableObject
    {
        [BoxGroup("战斗入口"), SerializeField, LabelText("初始牌组"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)] DeckPresetData _startingDeck;
        [BoxGroup("战斗入口"), SerializeField, LabelText("怪物列表"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)] MonsterListData _monsterList;
        [BoxGroup("战斗入口"), SerializeField, LabelText("奖励配置"), Required, InlineEditor(InlineEditorObjectFieldModes.Boxed)] BattleRewardConfigData _rewardConfig;

        [BoxGroup("玩家初始数值"), SerializeField, LabelText("最大能量"), MinValue(0)] int _maxEnergy = 3;

        [BoxGroup("运行设置"), SerializeField, LabelText("目标帧率"), MinValue(-1)] int _targetFrameRate = 60;

        public DeckPresetData StartingDeck => _startingDeck;
        public MonsterListData MonsterList => _monsterList;
        public BattleRewardConfigData RewardConfig => _rewardConfig;
        public int MaxEnergy => _maxEnergy;
        public int TargetFrameRate => _targetFrameRate;

        [BoxGroup("概览"), ShowInInspector, ReadOnly, MultiLineProperty(4), LabelText("配置摘要")]
        string InspectorSummary => $"初始牌组：{_startingDeck?.DeckName ?? "未配置"}\n怪物数：{_monsterList?.Monsters.Count ?? 0}\n奖励组数：{_rewardConfig?.RewardGroups.Count ?? 0}\n最大能量：{_maxEnergy}";
    }
}
