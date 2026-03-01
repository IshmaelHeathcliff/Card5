using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Skill
{
    public interface ISkillContainer
    {
        bool TryGetSkill(string id, out ISkill skill);
        ISkill GetSkill(string id);
        bool AddSkill(ISkill skill);
        bool RemoveSkill(string id, out ISkill skill);
        bool ReleaseSkill(string id, out ISkill skill);
        bool HasSkill(string id);
        bool HasSkills(IEnumerable<string> ids);
        int HasCount(IEnumerable<string> ids);
        int MaxCount { get; set; }
        int Count { get; }
        IEnumerable<ISkill> GetAllSkills();
        void Clear();
    }

    public class SkillContainer : ISkillContainer
    {
        readonly Dictionary<string, ISkill> _skills = new();
        public int MaxCount { get; set; }
        public int Count => _skills.Count;

        public SkillContainer(int maxCount = 0)
        {
            MaxCount = maxCount;
        }

        public IEnumerable<ISkill> GetAllSkills()
        {
            return _skills.Values;
        }

        public bool TryGetSkill(string id, out ISkill skill)
        {
            return _skills.TryGetValue(id, out skill);
        }

        public ISkill GetSkill(string id)
        {
            return _skills.GetValueOrDefault(id);
        }

        public bool AddSkill(ISkill skill)
        {
            // ! 同一ID的技能只能拥有一个
            if (_skills.TryAdd(skill.ID, skill))
            {
                // 默认获取时激活
                skill.Enable();
                return true;
            }

            return false;
        }

        public bool HasSkill(string id)
        {
            return _skills.ContainsKey(id);
        }

        public bool HasSkills(IEnumerable<string> ids)
        {
            return ids.All(HasSkill);
        }

        public int HasCount(IEnumerable<string> ids)
        {
            int count = 0;
            foreach (string id in ids)
            {
                if (HasSkill(id))
                {
                    count++;
                }
            }

            return count;
        }

        public bool RemoveSkill(string id, out ISkill skill)
        {
            if (_skills.TryGetValue(id, out skill))
            {
                skill.Disable();
                _skills.Remove(id);
                return true;
            }

            return false;
        }

        public bool ReleaseSkill(string id, out ISkill skill)
        {
            if (_skills.TryGetValue(id, out skill))
            {
                skill.Disable();
                _skills.Remove(id);
                return true;
            }

            return false;
        }

        public void Clear()
        {
            string[] skillIDs = _skills.Keys.ToArray();
            foreach (string id in skillIDs)
            {
                RemoveSkill(id, out _);
            }
        }
    }
}
