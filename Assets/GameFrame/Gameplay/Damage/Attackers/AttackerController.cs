using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data.Config;
using Gameplay.Character;
using Gameplay.Skill;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Gameplay.Damage.Attackers
{
    public interface IAttackerController : ICharacterControlled
    {
        UniTask<List<IAttacker>> GetAttackers(string skillID, string attackerID);
        void RemoveAttacker(IAttacker attacker);
        void ClearAttacker();
        bool CanAttack { get; set; }
    }

    public abstract class AttackerController : CharacterControlled, IController, IAttackerController
    {
        [SerializeField] string _targetTag;
        [SerializeField] AttackerPool _attackerPool;

        protected Transform AttackerParent { get; private set; }

        protected string TargetTag => _targetTag;
        protected ICharacterModel Model => CharacterController.CharacterModel;
        protected readonly List<IAttacker> Attackers = new();

        public bool CanAttack { get; set; } = true;

        AttackerSystem _attackerSystem;


        public virtual async UniTask<List<IAttacker>> GetAttackers(string skillID, string attackerID)
        {
            if (!CharacterController.CharacterModel.TryGetSkill(skillID, out ISkill skill))
            {
                Debug.LogError($"未获得ID为{skillID}的技能");
                return null;
            }

            if (skill is not AttackSkill attackSkill)
            {
                Debug.LogError($"ID为{skillID}的技能不是攻击技能");
                return null;
            }

            var attackers = (
                attackerID == "self" ?
                GetComponentsInChildren<IAttacker>() :
                (await GetAttacker(attackSkill, attackerID))
                ).ToList();

            foreach (IAttacker attacker in attackers)
            {
                attacker.AttackerController = this;
                attacker.TargetTag = TargetTag;

                if (!Attackers.Contains(attacker))
                {
                    Attackers.Add(attacker);
                }
            }

            return attackers;
        }

        async UniTask<IEnumerable<IAttacker>> GetAttacker(AttackSkill skill, string attackerID)
        {
            List<IAttacker> attackers = new();

            for (int i = 0; i < skill.ProjectileCount.Value; i++)
            {
                attackers.Add(await _attackerPool.Allocate(attackerID));
            }

            foreach (IAttacker attacker in attackers)
            {
                if (attacker is Attacker at)
                {
                    at.SetSkill(skill);
                    at.transform.SetParent(AttackerParent);
                }

            }

            return attackers;
        }

        public void RemoveAttacker(IAttacker attacker)
        {
            _attackerPool.Recycle(attacker.ID, attacker as Attacker);
            Attackers.Remove(attacker);
        }

        public virtual void ClearAttacker()
        {
            IAttacker[] attackers = Attackers.ToArray();
            foreach (IAttacker attacker in attackers)
            {
                attacker.Cancel().Forget();
            }
            Attackers.Clear();
        }

        /// <summary>
        /// 攻击技能获得时，将相应的 Attacker 加入到 AttackerPool 中
        /// </summary>
        void OnSkillAcquired(SkillAcquiredEvent e)
        {
            if (e.Model != Model)
            {
                return;
            }

            if (e.Skill is not AttackSkill attackSkill)
            {
                return;
            }

            if (attackSkill.SkillConfig is not AttackSkillConfig attackSkillConfig)
            {
                return;
            }

            foreach (AttackEffectConfig config in attackSkillConfig.AttackEffectConfigs)
            {
                string attackerID = config.AttackerID;

                if(attackerID == "self") continue;

                AttackerConfig attackerConfig = _attackerSystem.GetAttackerConfig(attackerID);
                _attackerPool.AddReference(attackerID, attackerConfig.Address).Forget();
            }
        }

        /// <summary>
        /// 攻击技能移除时，将相应的 Attacker 从 AttackerPool 中移除
        /// </summary>
        void OnSkillRemoved(SkillRemovedEvent e)
        {
            if (e.Model != Model)
            {
                return;
            }

            if (e.Skill is not AttackSkill attackSkill)
            {
                return;
            }

            if (attackSkill.SkillConfig is not AttackSkillConfig attackSkillConfig)
            {
                return;
            }

            foreach (AttackEffectConfig config in attackSkillConfig.AttackEffectConfigs)
            {
                if(config.AttackerID == "self") continue;
                _attackerPool.RemoveReference(config.AttackerID);
            }
        }

        protected override void OnInit()
        {
            _attackerSystem = this.GetSystem<AttackerSystem>();

            AttackerParent = transform.Find("Attackers");
            if (AttackerParent == null)
            {
                AttackerParent = new GameObject("Attackers").transform;
                AttackerParent.SetParent(transform);
            }

            this.RegisterEvent<SkillAcquiredEvent>(OnSkillAcquired).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SkillRemovedEvent>(OnSkillRemoved).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        protected override void OnDeinit()
        {
            ClearAttacker();
        }

        void OnValidate()
        {
            if (_attackerPool == null)
            {
                _attackerPool = GetComponentInChildren<AttackerPool>();
            }

            if (_attackerPool == null)
            {
                Debug.LogError("AttackerPool 组件未找到");
            }
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }

}
