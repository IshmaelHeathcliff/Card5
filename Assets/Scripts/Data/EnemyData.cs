using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Card5/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField, LabelText("敌人名称")] string _enemyName;
        [SerializeField, LabelText("最大生命值"), MinValue(1)] int _maxHp = 100;
        [SerializeField, LabelText("描述"), TextArea] string _description;
        [SerializeField, LabelText("立绘")] Sprite _portrait;

        public string EnemyName => _enemyName;
        public int MaxHp => _maxHp;
        public string Description => _description;
        public Sprite Portrait => _portrait;
    }
}
