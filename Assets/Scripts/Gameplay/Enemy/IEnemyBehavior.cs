namespace Card5
{
    /// <summary>
    /// 敌人行为接口，预留给状态机/行为树实现。
    /// EnemyController 后续可持有此接口实例来驱动 AI 行为。
    /// </summary>
    public interface IEnemyBehavior
    {
        void OnTurnStart();
        void OnTurnEnd();
        void OnTakeDamage(int amount);
    }
}
