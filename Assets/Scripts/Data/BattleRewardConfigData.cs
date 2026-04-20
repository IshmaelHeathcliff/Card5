using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "NewBattleRewardConfig", menuName = "Card5/Battle Reward Config")]
    public class BattleRewardConfigData : ScriptableObject
    {
        [SerializeField, ListDrawerSettings(ShowPaging = false)] List<BattleRewardGroupConfig> _rewardGroups = new List<BattleRewardGroupConfig>();

        public IReadOnlyList<BattleRewardGroupConfig> RewardGroups => _rewardGroups;
    }

    [Serializable]
    public class BattleRewardGroupConfig
    {
        [SerializeField] BattleRewardType _rewardType = BattleRewardType.Card;
        [SerializeField, MinValue(1)] int _choiceCount = 3;
        [SerializeField, ShowIf(nameof(IsCardReward)), ListDrawerSettings(ShowPaging = true)] List<CardData> _cardPool = new List<CardData>();

        public BattleRewardType RewardType => _rewardType;
        public int ChoiceCount => _choiceCount;
        public IReadOnlyList<CardData> CardPool => _cardPool;

        bool IsCardReward => _rewardType == BattleRewardType.Card;
    }
}
