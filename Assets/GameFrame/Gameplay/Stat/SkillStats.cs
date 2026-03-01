using System.Collections.Generic;
using System.Text;
using Data.Config;
using Data.SaveLoad;
using Gameplay.Skill;

namespace Gameplay.Stat
{
    public class SkillStats : Stats
    {
        public SkillStats(List<string> keywords, CharacterStats characterStats)
        {
            List<StatConfig> skillStats = SaveLoadManager.Load<List<StatConfig>>("SkillStats.json", "Preset");
            foreach (StatConfig stat in skillStats)
            {
                switch (stat.Type)
                {
                    case StatType.Consumable:
                        InternalStats.Add(stat.ID, new LocalConsumableStat(new ConsumableStat(stat.ID, stat.Name), characterStats.GetConsumableStat(stat.ID)));
                        break;
                    case StatType.Keyword:
                        InternalStats.Add(stat.ID, new LocalKeywordStat(keywords, new KeywordStat(stat.ID, stat.Name), characterStats.GetKeywordStat(stat.ID)));
                        break;
                    default:
                        InternalStats.Add(stat.ID, new LocalStat(new Stat(stat.ID, stat.Name), characterStats.GetStat(stat.ID)));
                        break;
                }
            }
        }

        public static StringBuilder GenerateSkillStatInfo(AttackSkill skill)
        {
            var info = new StringBuilder();
            info.Append($"{skill.Name}: \n");
            info.Append($"  Cooldown: {FormatStatValue(skill.Cooldown)}\n");
            foreach (IStat stat in skill.SkillStats.GetAllStats())
            {
                info.Append(GenerateStatInfo(stat, 1));
            }
            return info;
        }
    }
}


