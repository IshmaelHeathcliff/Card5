using System.Collections.Generic;

namespace Gameplay.Skill
{
    public interface ISkillReleaseReward
    {
        string Description { get; set; }
    }

    public class SpecificSkillsReleaseReward : ISkillReleaseReward
    {
        public List<string> NewSkillIDs { get; set; }
        public string Description { get; set; }

        public SpecificSkillsReleaseReward(List<string> newSkillIDs, string description = "")
        {
            NewSkillIDs = newSkillIDs;
            Description = description;
        }
    }
}
