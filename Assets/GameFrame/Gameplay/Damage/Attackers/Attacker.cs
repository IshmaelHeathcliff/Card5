using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Skill;
using Gameplay.Stat;
using UnityEngine;

namespace Gameplay.Damage.Attackers
{
    public interface IAttacker
    {
        string ID { get; }
        List<string> Keywords { get; }
        IAttackerController AttackerController { get; set; }
        string TargetTag { get; set; }
        Transform Target { get; set; }
        Vector2 Direction { get; set; }


        IKeywordStat Damage { get; }
        IKeywordStat CriticalChance { get; }
        IKeywordStat CriticalMultiplier { get; }
        IKeywordStat AttackArea { get; }
        IKeywordStat Duration { get; }

        void SetSkill(AttackSkill skill);
        UniTaskVoid Attack(CancellationToken cancellationToken);
        UniTaskVoid Cancel();
    }



    public abstract class Attacker : MonoBehaviour, IAttacker, IController
    {
        public IAttackerController AttackerController { get; set; }
        protected AttackSkill AttackSkill;

        // TODO: 需要一个更通用的方式来处理目标选择
        public string TargetTag { get; set; }
        public Transform Target { get; set; }
        public Vector2 Direction { get; set; }


        public string ID => AttackSkill.ID;
        public List<string> Keywords => AttackSkill.Keywords;
        public IKeywordStat Damage => AttackSkill.Damage;
        public IKeywordStat CriticalChance => AttackSkill.CriticalChance;
        public IKeywordStat CriticalMultiplier => AttackSkill.CriticalMultiplier;
        public IKeywordStat AttackArea => AttackSkill.AttackArea;
        public IKeywordStat Duration => AttackSkill.Duration;
        public IStat WoodOnUse => AttackSkill.WoodOnUse;

        public void SetSkill(AttackSkill skill)
        {
            AttackSkill = skill;
        }

        protected abstract UniTask Play(CancellationToken cancellationToken);
        public abstract UniTaskVoid Attack(CancellationToken cancellationToken);
        public abstract UniTaskVoid Cancel();

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
