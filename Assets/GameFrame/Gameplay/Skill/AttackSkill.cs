using Data.Config;
using Gameplay.Stat;

namespace Gameplay.Skill
{
    // 可以创建Attacker的Skill

    public class AttackSkill : RepetitiveSkill
    {
        public IKeywordStat Damage => SkillStats.GetKeywordStat("Damage");
        public IKeywordStat CriticalChance => SkillStats.GetKeywordStat("CriticalChance");
        public IKeywordStat CriticalMultiplier => SkillStats.GetKeywordStat("CriticalMultiplier");
        public IKeywordStat AttackArea => SkillStats.GetKeywordStat("AttackArea");
        public IKeywordStat Duration => SkillStats.GetKeywordStat("Duration");
        public IKeywordStat ProjectileCount => SkillStats.GetKeywordStat("ProjectileCount");
        public IKeywordStat ProjectileSpeed => SkillStats.GetKeywordStat("ProjectileSpeed");
        public IKeywordStat ChainCount => SkillStats.GetKeywordStat("ChainCount");
        public IKeywordStat PenetrateCount => SkillStats.GetKeywordStat("PenetrateCount");
        public IKeywordStat SplitCount => SkillStats.GetKeywordStat("SplitCount");
        public IStat WoodOnUse => SkillStats.GetStat("WoodOnUse");
        public bool IsTargetLocked { get; set; }
        public bool CanReturn { get; set; }

        public AttackSkill(AttackSkillConfig skillConfig, CharacterStats characterStats)
            : base(skillConfig, characterStats)
        {
            Damage.BaseValue = skillConfig.Damage;
            CriticalChance.BaseValue = skillConfig.CriticalChance;
            CriticalMultiplier.BaseValue = skillConfig.CriticalMultiplier;
            AttackArea.BaseValue = skillConfig.AttackArea;
            Duration.BaseValue = skillConfig.Duration;
            ProjectileCount.BaseValue = skillConfig.ProjectileCount;
            ProjectileSpeed.BaseValue = skillConfig.ProjectileSpeed;
            ChainCount.BaseValue = skillConfig.ChainCount;
            PenetrateCount.BaseValue = skillConfig.PenetrateCount;
            SplitCount.BaseValue = skillConfig.SplitCount;
            IsTargetLocked = skillConfig.IsTargetLocked;
            CanReturn = skillConfig.CanReturn;
            WoodOnUse.BaseValue = skillConfig.WoodOnUse;
        }
    }
}
