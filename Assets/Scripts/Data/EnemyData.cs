using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Card5/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] string _enemyName;
        [SerializeField, MinValue(1)] int _maxHp = 100;
        [SerializeField, TextArea] string _description;
        [SerializeField] Sprite _portrait;

        public string EnemyName => _enemyName;
        public int MaxHp => _maxHp;
        public string Description => _description;
        public Sprite Portrait => _portrait;
    }
}
