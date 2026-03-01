namespace Gameplay.Skill
{
    public class SkillReleaseRule
    {
        public string ID { get; set; }
        public ISkillReleaseCondition Condition { get; set; }
        public ISkillReleaseReward Reward { get; set; }
        public bool IsOneTime { get; set; } = true; // 是否只能触发一次
        public bool HasTriggered { get; set; } = false; // 是否已经触发过

        public SkillReleaseRule(string ruleID, ISkillReleaseCondition condition, ISkillReleaseReward reward = null, bool isOneTime = true)
        {
            ID = ruleID;
            Condition = condition;
            Reward = reward;
            IsOneTime = isOneTime;
            HasTriggered = false;
        }

        public string GetDescription()
        {
            return $"{ID} {Condition.Description} {Reward.Description}";
        }
    }
}
