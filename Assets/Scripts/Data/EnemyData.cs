using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Card5/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [BoxGroup("基础信息文本"), SerializeField, LabelText("敌人名称")] string _enemyName;
        [BoxGroup("基础信息文本"), SerializeField, LabelText("最大生命值"), MinValue(1)] int _maxHp = 100;
        [BoxGroup("基础信息文本"), SerializeField, LabelText("描述"), TextArea(3, 6)] string _description;
        [BoxGroup("基础信息视觉"), SerializeField, LabelText("立绘"), PreviewField(100, ObjectFieldAlignment.Center)] Sprite _portrait;

        public string EnemyName => _enemyName;
        public int MaxHp => _maxHp;
        public string Description => _description;
        public Sprite Portrait => _portrait;

        [BoxGroup("基础信息说明"), ShowInInspector, ReadOnly, MultiLineProperty(4), LabelText("预览说明")]
        string InspectorSummary => $"[{_enemyName}] HP {_maxHp}\n{_description}";
    }
}
