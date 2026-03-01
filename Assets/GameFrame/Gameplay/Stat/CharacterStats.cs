using System.Collections.Generic;
using System.Linq;
using Data.Config;
using Data.SaveLoad;

namespace Gameplay.Stat
{
    public class CharacterStats : Stats
    {
        public CharacterStats()
        {
            List<StatConfig> characterStats = SaveLoadManager.Load<List<StatConfig>>("CharacterStats.json", "Preset");
            List<StatConfig> skillStats = SaveLoadManager.Load<List<StatConfig>>("SkillStats.json", "Preset");
            foreach (StatConfig stat in characterStats.Concat(skillStats))
            {
                switch (stat.Type)
                {
                    case StatType.Consumable:
                        InternalStats.Add(stat.ID, new ConsumableStat(stat.ID, stat.Name));
                        break;
                    case StatType.Keyword:
                        InternalStats.Add(stat.ID, new KeywordStat(stat.ID, stat.Name));
                        break;
                    default:
                        InternalStats.Add(stat.ID, new Stat(stat.ID, stat.Name));
                        break;
                }
            }
        }
    }
}
