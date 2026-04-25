using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Card5
{
    [CreateAssetMenu(fileName = "NewMonsterList", menuName = "Card5/Monster List")]
    public class MonsterListData : ScriptableObject
    {
        [SerializeField, LabelText("怪物列表"), ListDrawerSettings(ShowPaging = false, DraggableItems = true)] List<MonsterStageConfig> _monsters = new List<MonsterStageConfig>();

        public IReadOnlyList<MonsterStageConfig> Monsters => _monsters;
    }

    [Serializable]
    public class MonsterStageConfig
    {
        [SerializeField, LabelText("怪物配置"), Required] EnemyData _enemyData;
        [SerializeField, LabelText("最大出牌轮数"), MinValue(1)] int _maxPlayRounds = 5;

        public EnemyData EnemyData => _enemyData;
        public int MaxPlayRounds => _maxPlayRounds;
    }
}
