using System.Collections.Generic;
using Data.Config;
using Gameplay.Character;

namespace Gameplay.Skill
{
    public interface ISkillEvent
    {
        ISkillContainer SkillsInSlot { get; set; }
        ISkillContainer SkillsInRelease { get; set; }
    }

    public abstract class SkillEvent : ISkillEvent
    {
        public ICharacterModel Model { get; set; }
        public ISkillContainer SkillsInSlot { get; set; }
        public ISkillContainer SkillsInRelease { get; set; }

        public SkillEvent(ICharacterModel model)
        {
            Model = model;
            SkillsInSlot = model.SkillsInSlot;
            SkillsInRelease = model.SkillsReleased;
        }
    }

    public class SkillSlotCountChangedEvent : SkillEvent
    {
        public int Count { get; set; }

        public SkillSlotCountChangedEvent(int count, ICharacterModel model) : base(model)
        {
            Count = count;
        }
    }

    public class SkillReleasedEvent : SkillEvent
    {
        public ISkill Skill { get; set; }

        public SkillReleasedEvent(ISkill skill, ICharacterModel model) : base(model)
        {
            Skill = skill;
        }
    }

    public class SkillAcquiredEvent : SkillEvent, IReleaseEvent
    {
        public ISkill Skill { get; set; }

        public SkillAcquiredEvent(ISkill skill, ICharacterModel model) : base(model)
        {
            Skill = skill;
        }
    }

    public class SkillRemovedEvent : SkillEvent
    {
        public ISkill Skill { get; set; }

        public SkillRemovedEvent(ISkill skill, ICharacterModel model) : base(model)
        {
            Skill = skill;
        }
    }

    public class GachaSkillsEvent : SkillEvent
    {
        public List<SkillConfig> Skills { get; set; }

        public GachaSkillsEvent(List<SkillConfig> skills, ICharacterModel model) : base(model)
        {
            Skills = skills;
        }
    }

    public class SelectSkillEvent : SkillEvent
    {
        public List<SkillConfig> Skills { get; set; }
        public int Index { get; set; }

        public SelectSkillEvent(List<SkillConfig> skills, int index, ICharacterModel model) : base(model)
        {
            Skills = skills;
            Index = index;
        }
    }

    public class FullSlotWhenAcquireSkillEvent : SkillEvent
    {
        public ISkill Skill { get; set; }

        public FullSlotWhenAcquireSkillEvent(ISkill skill, ICharacterModel model) : base(model)
        {
            Skill = skill;
        }
    }
}
