using Gameplay.Character;
using Gameplay.Stat;
using UnityEngine;

namespace Gameplay.Damage
{
    public interface IDamageable : ICharacterControlled
    {
        string ID { get; }
        EasyEvent OnHurt { get; }
        EasyEvent OnDeath { get; }
        Transform Transform { get; }
        IConsumableStat Health { get; }
        bool IsDamageable { get; set; }
        void TakeDamage(float damage);

    }
    public abstract class Damageable : CharacterControlled, IDamageable, IController
    {

        public string ID => CharacterController.CharacterModel.ID;
        public EasyEvent OnHurt { get; protected set; }
        public EasyEvent OnDeath { get; protected set; }
        public Transform Transform { get; protected set; }
        public IConsumableStat Health { get; protected set; }
        public bool IsDamageable { get; set; } = true;

        protected override void OnInit()
        {
            Transform = transform;
        }

        protected override void OnDeinit()
        {
        }

        public void SetStats(Stats stats)
        {
            Health = stats.GetStat("Health") as IConsumableStat;
        }

        public abstract void TakeDamage(float damage);


        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
