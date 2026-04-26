using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "NewMonsterList", menuName = "Card5/Monster List")]
    public class MonsterListData : ScriptableObject
    {
        [BoxGroup("概览"), ShowInInspector, ReadOnly, LabelText("怪物数量")]
        int MonsterCount => _monsters.Count;

        [BoxGroup("概览"), ShowInInspector, ReadOnly, LabelText("总轮数")]
        int TotalPlayRounds => _monsters.Sum(monster => monster != null ? monster.MaxPlayRounds : 0);

        [BoxGroup("怪物列表")]
        [SerializeField, LabelText("怪物列表"), ListDrawerSettings(ShowPaging = false, DraggableItems = true, DefaultExpandedState = true)]
        [Searchable]
        List<MonsterStageConfig> _monsters = new List<MonsterStageConfig>();

        public IReadOnlyList<MonsterStageConfig> Monsters => _monsters;
    }

    [Serializable]
    public class MonsterStageConfig
    {
        [BoxGroup("基础信息"), SerializeField, LabelText("怪物配置"), Required] EnemyData _enemyData;
        [BoxGroup("基础信息"), SerializeField, LabelText("最大出牌轮数"), MinValue(1)] int _maxPlayRounds = 5;
        [BoxGroup("基础信息"), ShowInInspector, ReadOnly, PreviewField(80, ObjectFieldAlignment.Left), LabelText("立绘预览")]
        Sprite PortraitPreview => _enemyData != null ? _enemyData.Portrait : null;
        [BoxGroup("基础信息"), ShowInInspector, ReadOnly, LabelText("阶段说明")]
        string Summary => $"{_enemyData?.EnemyName ?? "未配置怪物"}，最多 {_maxPlayRounds} 轮";

        public EnemyData EnemyData => _enemyData;
        public int MaxPlayRounds => _maxPlayRounds;

        public override string ToString()
        {
            return Summary;
        }
    }
}
