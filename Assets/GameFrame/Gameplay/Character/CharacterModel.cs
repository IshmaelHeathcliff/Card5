using System.Collections.Generic;
using System.Linq;
using Gameplay.Items;
using Gameplay.Skill;
using Gameplay.Stat;
using Gameplay.Status;
using UnityEngine;


namespace Gameplay.Character
{
    public interface IHasSkill
    {
        SkillPool SkillPool { get; set; }
        ISkillContainer SkillsReleased { get; }
        ISkillContainer SkillsInSlot { get; }
        int SkillSlotCount { get; set; }
        ISkill GetSkill(string id);
        bool TryGetSkill(string id, out ISkill skill);
        IEnumerable<ISkill> GetAllSkills();
        bool HasSkills(IEnumerable<string> ids);
    }

    public interface IHasStatus
    {
        IStatusContainer StatusContainer { get; }
    }

    public interface ICanCountValue
    {
        Dictionary<string, ValueCounter> CountValues { get; }
    }

    public interface IHasResources
    {
        IResourceContainer Resources { get; }
    }

    public interface ICharacterModel : IHasSkill, IHasStatus, ICanCountValue
    {
        ICharacterController Controller { get; set; }

        string ID { get; set; }
        Transform Transform { get; set; }

        float Speed { get; set; }
        Vector2 Position { get; set; }
        Vector2 Direction { get; set; }

        CharacterStats Stats { get; }
    }

    public abstract class CharacterModel : ICharacterModel
    {
        public ICharacterController Controller { get; set; }

        public string ID { get; set; }

        public Transform Transform { get; set; }

        public float Speed { get; set; }

        public Vector2 Position
        {
            get
            {
                if (Transform == null)
                {
                    return Vector2.zero;
                }

                return Transform.position;
            }
            set
            {
                if (Transform == null)
                {
                    return;
                }

                Transform.position = value;
            }
        }

        public Vector2 Direction { get; set; }

        public CharacterStats Stats { get; } = new CharacterStats();

        public IStatusContainer StatusContainer { get; } = new StatusContainer();

        public Dictionary<string, ValueCounter> CountValues { get; } = new();


        #region IHasSkill
        public SkillPool SkillPool { get; set; } = new();

        public int SkillSlotCount
        {
            get => SkillsInSlot.MaxCount;
            set => SkillsInSlot.MaxCount = value;
        }

        public ISkillContainer SkillsReleased { get; } = new SkillContainer();
        public ISkillContainer SkillsInSlot { get; } = new SkillContainer(7);

        public ISkill GetSkill(string id)
        {
            if (SkillsInSlot.TryGetSkill(id, out ISkill skill))
            {
                return skill;
            }

            if (SkillsReleased.TryGetSkill(id, out skill))
            {
                return skill;
            }

            return null;
        }

        public bool TryGetSkill(string id, out ISkill skill)
        {
            return SkillsInSlot.TryGetSkill(id, out skill) || SkillsReleased.TryGetSkill(id, out skill);
        }

        public IEnumerable<ISkill> GetAllSkills()
        {
            return SkillsInSlot.GetAllSkills().Concat(SkillsReleased.GetAllSkills());
        }

        public bool HasSkills(IEnumerable<string> ids)
        {
            foreach (string id in ids)
            {
                if (!(SkillsInSlot.HasSkill(id) || SkillsReleased.HasSkill(id)))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

    }
}
