using UnityEngine;

namespace Card5
{
    /// <summary>
    /// 卡牌效果基类，所有效果继承此类并实现 Execute。
    /// 新增效果只需继承此类并添加 [CreateAssetMenu]。
    /// </summary>
    public abstract class CardEffectSO : ScriptableObject
    {
        [SerializeField] string _description;

        public string Description => _description;

        public abstract void Execute(BattleContext context);

        public virtual string GetDescription() => _description;
    }
}
