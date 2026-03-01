using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gameplay.Character.Player;
using Gameplay.Stat;
using Gameplay.Skill;
using TMPro;
using UnityEngine;

namespace UI
{
    public class SkillSlotUIController : MonoBehaviour, IController
    {
        [SerializeField] List<SkillSlotUI> _skillSlotUIs;
        [SerializeField] GameObject _skillInfo;
        TextMeshProUGUI _skillInfoText;

        PlayerModel _playerModel;
        SkillSystem _skillSystem;

        ISkill _skillToReplace;

        void Init()
        {
            foreach (ISkill skill in _playerModel.SkillsInSlot.GetAllSkills())
            {
                AddSkill(skill);
            }
        }

        void OnSkillAcquired(SkillAcquiredEvent e)
        {
            if (e.Model != _playerModel)
            {
                return;
            }

            if (!_playerModel.SkillsInSlot.HasSkill(e.Skill.ID))
            {
                return;
            }

            AddSkill(e.Skill);
        }

        void AddSkill(ISkill skill)
        {
            foreach (SkillSlotUI slot in _skillSlotUIs)
            {
                if (slot.Skill == null)
                {
                    slot.SetSkill(skill);
                    break;
                }
            }
        }

        void OnSkillReleased(SkillReleasedEvent e)
        {
            if (e.Model != _playerModel)
            {
                return;
            }

            foreach (SkillSlotUI slot in _skillSlotUIs)
            {
                if (slot.Skill == e.Skill)
                {
                    slot.SetSkill();
                    break;
                }
            }

        }

        void OnSkillRemoved(SkillRemovedEvent e)
        {
            if (e.Model != _playerModel)
            {
                return;
            }

            foreach (SkillSlotUI slot in _skillSlotUIs)
            {
                if (slot.Skill != null && slot.Skill == e.Skill)
                {
                    slot.SetSkill();
                    break;
                }
            }
        }

        void OnFullSlotWhenAcquireSkill(FullSlotWhenAcquireSkillEvent e)
        {
            if (e.Model != _playerModel)
            {
                return;
            }

            foreach (SkillSlotUI slot in _skillSlotUIs)
            {
                slot.IsRemovable = true;
            }

            _skillToReplace = e.Skill;
        }

        void ReplaceSkill(ISkill oldSkill)
        {
            _skillSystem.RemoveSkill(oldSkill.ID, _playerModel);

            if (_skillToReplace != null)
            {
                _skillSystem.AcquireSkill(_skillToReplace.ID, _playerModel);
                _skillToReplace = null;
            }

            foreach (SkillSlotUI slot in _skillSlotUIs)
            {
                slot.IsRemovable = false;
            }
        }

        void OnPointerEnter(ISkill skill)
        {
            // _skillInfo.SetActive(true);

            if (skill == null)
            {
                _skillInfoText.text = "";
                return;
            }
            else
            {
                // var info = new StringBuilder();
                // info.Append(skill.Description);

                // if (skill is AttackSkill attackSkill)
                // {
                //     info.AppendLine("");
                //     info.Append(SkillStats.GenerateSkillStatInfo(attackSkill));
                // }

                // _skillInfoText.text = info.ToString();
                _skillInfoText.text = skill.Name + "\n\n" + skill.Description;
            }
        }

        void OnPointerExit(ISkill skill)
        {
            // _skillInfo.SetActive(false);
            // _skillInfoText.text = "";
        }

        void OnValidate()
        {
            _skillSlotUIs = GetComponentsInChildren<SkillSlotUI>().ToList();
        }

        void Awake()
        {
            _skillInfoText = _skillInfo.GetComponentInChildren<TextMeshProUGUI>();
        }

        void Start()
        {
            _playerModel = this.GetModel<PlayersModel>().Current;
            _skillSystem = this.GetSystem<SkillSystem>();

            this.RegisterEvent<SkillAcquiredEvent>(OnSkillAcquired).UnRegisterWhenDisabled(this);
            this.RegisterEvent<SkillReleasedEvent>(OnSkillReleased).UnRegisterWhenDisabled(this);
            this.RegisterEvent<SkillRemovedEvent>(OnSkillRemoved).UnRegisterWhenDisabled(this);
            this.RegisterEvent<FullSlotWhenAcquireSkillEvent>(OnFullSlotWhenAcquireSkill).UnRegisterWhenDisabled(this);

            foreach (SkillSlotUI slot in _skillSlotUIs)
            {
                slot.OnSkillPointerEnter.Register(OnPointerEnter).UnRegisterWhenDisabled(this);
                slot.OnSkillPointerExit.Register(OnPointerExit).UnRegisterWhenDisabled(this);
                slot.OnSkillReplace.Register(ReplaceSkill).UnRegisterWhenDisabled(this);
            }

            Init();
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
