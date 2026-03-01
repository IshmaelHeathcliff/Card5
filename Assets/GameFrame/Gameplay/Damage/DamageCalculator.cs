using System.Collections.Generic;
using Gameplay.Damage.Attackers;

namespace Gameplay.Damage
{
    public abstract class DamageCalculator
    {
        protected IAttacker Attacker { get; set; }
        protected IDamageable Damageable { get; set; }
        protected List<string> Keywords { get; set; }
        protected float BaseDamage { get; set; }
        protected float AddedMultiplier { get; set; }

        public abstract float Calculate();

        public DamageCalculator(IAttacker attacker, IDamageable damageable, float baseDamage, List<string> keywords, float addedMultiplier = 1)
        {
            Attacker = attacker;
            Damageable = damageable;
            BaseDamage = baseDamage;
            AddedMultiplier = addedMultiplier;
            Keywords = keywords;
        }
    }

    public class SimpleHitCalculator : DamageCalculator
    {
        public SimpleHitCalculator(IAttacker attacker, IDamageable damageable, float baseDamage, List<string> keywords, float addedMultiplier) : base(attacker, damageable, baseDamage, keywords, addedMultiplier)
        {
        }

        public override float Calculate()
        {
            return Attacker.Damage.GetValueByKeywords(BaseDamage, Keywords, AddedMultiplier);
        }
    }
}
