using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "NewBattleRewardConfig", menuName = "Card5/Battle Reward Config")]
    public class BattleRewardConfigData : ScriptableObject
    {
        [BoxGroup("概览"), ShowInInspector, ReadOnly, LabelText("奖励组数量")]
        int RewardGroupCount => _rewardGroups.Count;

        [BoxGroup("奖励组")]
        [SerializeField, LabelText("奖励组"), ListDrawerSettings(ShowPaging = false, DefaultExpandedState = true, DraggableItems = true)]
        List<BattleRewardGroupConfig> _rewardGroups = new List<BattleRewardGroupConfig>();

        public IReadOnlyList<BattleRewardGroupConfig> RewardGroups => _rewardGroups;
    }

    [Serializable]
    public class BattleRewardGroupConfig
    {
        [BoxGroup("基础配置"), SerializeField, LabelText("奖励类型")] BattleRewardType _rewardType = BattleRewardType.Card;
        [BoxGroup("基础配置"), SerializeField, LabelText("可选数量"), MinValue(1)] int _choiceCount = 3;
        [BoxGroup("基础配置"), ShowInInspector, ReadOnly, LabelText("奖励说明")]
        string GroupSummary => GetSummary();

        [BoxGroup("卡牌奖励"), SerializeField, LabelText("卡牌牌库"), ShowIf(nameof(IsCardReward)), InlineEditor(InlineEditorObjectFieldModes.Boxed)] CardLibraryData _cardLibrary;
        [BoxGroup("卡牌奖励"), SerializeField, LabelText("兼容卡池"), ShowIf(nameof(UsesLegacyCardPool)), ListDrawerSettings(ShowPaging = true, DefaultExpandedState = true)]
        [Searchable]
        List<CardData> _cardPool = new List<CardData>();

        public BattleRewardType RewardType => _rewardType;
        public int ChoiceCount => _choiceCount;
        public CardLibraryData CardLibrary => _cardLibrary;
        public IReadOnlyList<CardData> CardPool => _cardPool;

        bool IsCardReward => _rewardType == BattleRewardType.Card;
        bool UsesLegacyCardPool => IsCardReward && _cardLibrary == null;

        string GetSummary()
        {
            if (_rewardType == BattleRewardType.Card)
            {
                if (_cardLibrary != null)
                {
                    return $"卡牌奖励，共 {_choiceCount} 选，来源为牌库";
                }

                return $"卡牌奖励，共 {_choiceCount} 选，兼容卡池 {_cardPool.Count} 张";
            }

            return $"{_rewardType} 奖励，共 {_choiceCount} 选";
        }

        public override string ToString()
        {
            return GetSummary();
        }
    }
}
