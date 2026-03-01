using System.Collections.Generic;
using Data.Config;
using Data.SaveLoad;
using UnityEngine;
using System.Linq;
using Gameplay.Character;
using Gameplay.Items;
using Gameplay.Modifier;
using Gameplay.Status;

namespace Gameplay.Skill
{
    public class SkillCreateEnv
    {
        public ICharacterModel Model;
        public ModifierSystem ModifierSystem;
        public ResourceSystem ResourceSystem;
        public CountSystem CountSystem;
        public SkillSystem SkillSystem;
        public StatusCreateSystem StatusCreateSystem;
    }

    public class SkillSystem : AbstractSystem
    {
        readonly Dictionary<string, SkillConfig> _skillConfigCache = new();
        const string JsonPath = "Preset";
        const string JsonName = "Skills.json";

        bool _isLoaded = false;

        readonly SkillConfigLoader _skillConfigLoader = new();

        SkillCreateEnv SkillCreateEnv { get; set; }

        void Load()
        {
            _skillConfigCache.Clear();
            List<SkillConfig> skillConfigList = this.GetUtility<SaveLoadUtility>().Load<List<SkillConfig>>(JsonName, JsonPath);
            foreach (SkillConfig skillConfig in skillConfigList)
            {
                _skillConfigCache.Add(skillConfig.ID, skillConfig);
            }

            _isLoaded = true;
        }

        public SkillConfig GetSkillConfig(string id)
        {
            if (!_isLoaded)
            {
                Load();
            }

            if (_skillConfigCache.TryGetValue(id, out SkillConfig skillConfig))
            {
                return skillConfig;
            }

            Debug.LogError($"SkillConfig not found: {id}");
            return null;
        }

        public IEnumerable<SkillConfig> GetSkillConfigs(IEnumerable<string> ids)
        {
            return ids.Select(GetSkillConfig).Where(config => config != null);
        }

        public void SetEnv(ICharacterModel model)
        {
            if (model != null)
            {
                SkillCreateEnv.Model = model;
            }

            if (!CheckEnv())
            {
                Debug.LogError("SkillCreateEnv is not set correctly");
            }
        }

        public ISkill CreateSkill(string id, ICharacterModel model = null)
        {
            SetEnv(model);

            return _skillConfigLoader.CreateSkill(GetSkillConfig(id), SkillCreateEnv);
        }

        public void AcquireSkill(string id, ICharacterModel model = null)
        {
            SetEnv(model);

            if (SkillCreateEnv.Model.SkillsReleased.HasSkill(id))
            {
                return;
            }

            ISkill skill = CreateSkill(id, model);

            ISkillContainer skillsInSlot = SkillCreateEnv.Model.SkillsInSlot;

            if (skillsInSlot.Count >= skillsInSlot.MaxCount)
            {
                this.SendEvent(new FullSlotWhenAcquireSkillEvent(skill, SkillCreateEnv.Model));
                Debug.Log($"技能槽位已满，最大数量: {skillsInSlot.MaxCount}");
                return;
            }

            if (!skillsInSlot.AddSkill(skill))
            {
                return;
            }

            this.SendEvent(new SkillAcquiredEvent(skill, SkillCreateEnv.Model));

            if (skill is ISkill { ReleaseOnAcquire: true })
            {
                ReleaseSkill(id, model);
            }
        }

        public void ReleaseSkill(string id, ICharacterModel model = null)
        {
            SetEnv(model);

            if (!SkillCreateEnv.Model.SkillsInSlot.ReleaseSkill(id, out ISkill skill))
            {
                return;
            }

            if (!SkillCreateEnv.Model.SkillsReleased.AddSkill(skill))
            {
                return;
            }

            this.SendEvent(new SkillReleasedEvent(skill, SkillCreateEnv.Model));
        }

        public void RemoveSkill(string id, ICharacterModel model = null)
        {
            SetEnv(model);

            if (SkillCreateEnv.Model.SkillsInSlot.RemoveSkill(id, out ISkill skill) || SkillCreateEnv.Model.SkillsReleased.RemoveSkill(id, out skill))
            {
                this.SendEvent(new SkillRemovedEvent(skill, SkillCreateEnv.Model));
            }
        }

        public void ClearSkill(ICharacterModel model)
        {
            foreach (ISkill skill in model.GetAllSkills())
            {
                this.SendEvent(new SkillRemovedEvent(skill, model));
            }

            model.SkillsInSlot.Clear();
            model.SkillsReleased.Clear();
        }

        public void SetSkillSlotCount(int count, ICharacterModel model = null)
        {
            SetEnv(model);
            SkillCreateEnv.Model.SkillSlotCount = count;
            this.SendEvent(new SkillSlotCountChangedEvent(count, SkillCreateEnv.Model));
        }

        public bool CheckEnv()
        {
            if (SkillCreateEnv.Model == null)
            {
                Debug.LogError("Model is null");
                return false;
            }

            return true;
        }

        protected override void OnInit()
        {
            Load();
            SkillCreateEnv = new SkillCreateEnv
            {
                ModifierSystem = this.GetSystem<ModifierSystem>(),
                ResourceSystem = this.GetSystem<ResourceSystem>(),
                CountSystem = this.GetSystem<CountSystem>(),
                SkillSystem = this,
                StatusCreateSystem = this.GetSystem<StatusCreateSystem>()
            };
        }
    }
}
