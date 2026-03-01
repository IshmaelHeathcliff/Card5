using System.Collections.Generic;
using Gameplay.Damage.Attackers;
using UnityEngine;

namespace Gameplay.Damage
{
    public enum DamageType
    {
        Simple,
    }

    public abstract class Damage
    {
        public List<string> Keywords { get; set; }
        public IAttacker Attacker { get; set; }
        public IDamageable Damageable { get; set; }

        protected DamageType Type;
        protected float BaseDamage;
        protected float AddedMultiplier;

        protected DamageCalculator DamageCalculator;

        public virtual void Apply()
        {
            float damage = DamageCalculator.Calculate();
            Damageable.TakeDamage(damage);
        }

        protected Damage(IAttacker attacker, IDamageable damageable, List<string> keywords, DamageType type, float baseDamage, float addedMultiplier)
        {
            Keywords = keywords;
            Attacker = attacker;
            Damageable = damageable;
            Type = type;
            BaseDamage = baseDamage;
            AddedMultiplier = addedMultiplier;
        }

    }

    public class AttackDamage : Damage
    {
        float _baseMultiplier;

        public override void Apply()
        {
            float damage = DamageCalculator.Calculate();

            if (IsCritical())
            {
                float criticalMultiplier = Attacker.CriticalMultiplier.Value;
                damage *= criticalMultiplier / 100f;
            }

            // Debug.Log($"AttackDamage: {damage}");
            Damageable.TakeDamage(damage);
        }

        bool IsCritical()
        {
            float critical = Attacker.CriticalChance.Value;

            switch (critical)
            {
                case < 0:
                    Debug.LogError("Critical chance cannot be negative");
                    return false;
                case >= 100f:
                    return true;
                default:
                    return Random.Range(0f, 1f) < critical / 100f;
            }
        }

        public AttackDamage(IAttacker attacker, IDamageable damageable, List<string> keywords, DamageType type, float baseDamage, float addedMultiplier, float baseMultiplier) : base(attacker, damageable, keywords, type, baseDamage, addedMultiplier)
        {
            _baseMultiplier = baseMultiplier;
            DamageCalculator = type switch
            {
                _ => new SimpleHitCalculator(attacker, damageable, baseDamage * baseMultiplier, keywords, addedMultiplier),
            };
        }
    }

    public class DamageOverTime : Damage
    {
        float _duration;

        public DamageOverTime(IAttacker attacker, IDamageable damageable, List<string> keywords, DamageType type, float baseDamage, float addedMultiplier) : base(attacker, damageable, keywords, type, baseDamage, addedMultiplier)
        {
        }
    }
}
