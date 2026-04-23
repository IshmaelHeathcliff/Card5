using UnityEngine;

namespace Card5
{
    public enum CardEffectBoostMode
    {
        [InspectorName("固定增加")]
        AddFlat,
        [InspectorName("百分比增加")]
        AddPercent,
        [InspectorName("倍率提升")]
        Multiply
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

        public float Apply(float amount)
        {
            return Mode switch
            {
                CardEffectBoostMode.AddFlat    => amount + Value,
                CardEffectBoostMode.AddPercent => amount * (1f + Value * 0.01f),
                CardEffectBoostMode.Multiply   => amount * Value,
                _                              => amount
            };
        }
    }
}
