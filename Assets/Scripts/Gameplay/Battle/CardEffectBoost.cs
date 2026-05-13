using UnityEngine;

namespace Card5
{
    public enum CardEffectBoostMode
    {
        [InspectorName("基础伤害增加")]
        AddFlat,
        [InspectorName("伤害百分比增减")]
        AddPercent,
        [InspectorName("伤害总增")]
        Multiply,
        [InspectorName("固定值增加")]
        AddFixed
    }

    public readonly struct CardEffectBoost
    {
        public CardEffectBoost(CardEffectBoostMode mode, float value)
        {
            Mode = mode;
            Value = value;
        }

        public CardEffectBoostMode Mode { get; }
        public float Value { get; }

        public void ApplyTo(ref DamageModifier modifier)
        {
            modifier.Apply(this);
        }
    }

    public struct DamageModifier
    {
        public float BaseDamage { get; private set; }
        public float DamageIncreasePercent { get; private set; }
        public float TotalMultiplier { get; private set; }
        public float FixedValue { get; private set; }

        public DamageModifier(float baseDamage)
        {
            BaseDamage = baseDamage;
            DamageIncreasePercent = 0f;
            TotalMultiplier = 1f;
            FixedValue = 0f;
        }

        public void Apply(CardEffectBoost boost)
        {
            switch (boost.Mode)
            {
                case CardEffectBoostMode.AddFlat:
                    BaseDamage += boost.Value;
                    break;
                case CardEffectBoostMode.AddPercent:
                    DamageIncreasePercent += boost.Value;
                    break;
                case CardEffectBoostMode.Multiply:
                    TotalMultiplier *= boost.Value;
                    break;
                case CardEffectBoostMode.AddFixed:
                    FixedValue += boost.Value;
                    break;
            }
        }

        public float Calculate()
        {
            return BaseDamage * (1f + DamageIncreasePercent * 0.01f) * TotalMultiplier + FixedValue;
        }
    }
}
