using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Character;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Skill
{
    [RequireComponent(typeof(ICharacterController))]
    public class SkillController : MonoBehaviour, IController
    {
        [SerializeField] string _initSkill;
        [SerializeField] int _skillSlotCount;
        readonly Dictionary<string, RepetitiveSkill> _repetitiveSkills = new();

        ICharacterController _characterController;
        ICharacterModel _model;

        void OnSkillAcquired(SkillAcquiredEvent e)
        {
            if (e.Model != _model)
            {
                return;
            }

            if (e.Skill is not RepetitiveSkill repetitiveSkill)
            {
                return;
            }

            _repetitiveSkills.TryAdd(repetitiveSkill.ID, repetitiveSkill);
        }

        void OnSkillReleased(SkillReleasedEvent e)
        {
            if (e.Model != _model)
            {
                return;
            }
        }

        void OnSkillRemoved(SkillRemovedEvent e)
        {
            if (e.Model != _model)
            {
                return;
            }

            _repetitiveSkills.Remove(e.Skill.ID);
        }

        [Button]
        void CreateSkill(string skillID)
        {
            this.SendCommand(new AcquireSkillCommand(skillID, _model));
        }

        void Awake()
        {
            _characterController = GetComponent<ICharacterController>();
        }

        void Start()
        {
            _model = _characterController.CharacterModel;

            // ! 初始化技能系统
            this.SendCommand(new SetSkillSlotCountCommand(_skillSlotCount, _model));

            this.RegisterEvent<SkillAcquiredEvent>(OnSkillAcquired).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SkillReleasedEvent>(OnSkillReleased).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SkillRemovedEvent>(OnSkillRemoved).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.SendCommand(new AcquireSkillCommand(_initSkill, _model));
        }


        void Update()
        {
            foreach (RepetitiveSkill skill in _repetitiveSkills.Values)
            {
                skill.Update(Time.deltaTime);

                // 自动释放
                if (skill.IsReady)
                {
                    skill.Use();
                }
            }
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
