using System;
using Data.Config;
using Gameplay.Stat;
using Newtonsoft.Json;
using Random = UnityEngine.Random;

namespace Gameplay.Modifier
{
    [Serializable]
    public class StatSingleFloatModifier : StatModifier<float>
    {
        [JsonConstructor]
        public StatSingleFloatModifier()
        {

        }

        public StatSingleFloatModifier(StatModifierConfig modifierConfig, IStat stat) : base(modifierConfig, stat)
        {
        }

        public StatSingleFloatModifier(StatModifierConfig modifierConfig, IStat stat, float value) : base(modifierConfig,
            stat)
        {
            Level = 1;
            Value = value;
        }

        public override string GetDescription()
        {
            return Value >= 0 ?
                string.Format(ModifierConfig.PositiveDescription, Stat.ID, Value) :
                string.Format(ModifierConfig.NegativeDescription, Stat.ID, -Value);
        }

        public override void Check()
        {
            // TODO: 检查Modifier生效条件
            throw new NotImplementedException();
        }

        public override void Register()
        {
            switch (ModifierConfig.StatModifierType)
            {
                case StatModifierType.Added:
                    Stat.AddAddedValueModifier(InstanceID, this);
                    break;
                case StatModifierType.Increase:
                    Stat.AddIncreaseModifier(InstanceID, this);
                    break;
                case StatModifierType.More:
                    Stat.AddMoreModifier(InstanceID, this);
                    break;
                case StatModifierType.Fixed:
                    Stat.AddFixedValueModifier(InstanceID, this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public override void Unregister()
        {
            switch (ModifierConfig.StatModifierType)
            {
                case StatModifierType.Added:
                    Stat.RemoveAddedValueModifier(InstanceID);
                    break;
                case StatModifierType.Increase:
                    Stat.RemoveIncreaseModifier(InstanceID);
                    break;
                case StatModifierType.More:
                    Stat.RemoveMoreModifier(InstanceID);
                    break;
                case StatModifierType.Fixed:
                    Stat.RemoveFixedValueModifier(InstanceID);
                    break;
                default:
                    break;
            }

        }

        public override void Load()
        {
            ModifierConfig = GetModifierConfig(ModifierID);
            Stat = GetStat(this);

        }

        public override void RandomizeLevel()
        {
            if (ModifierConfig is StatModifierConfig config)
            {
                Level = Random.Range(0, config.MaxLevel);
            }

        }

        public override void RandomizeValue()
        {
            if (ModifierConfig is StatModifierConfig config)
            {
                LevelRange levelRange = config.LevelRanges[Level];
                Value = Random.Range(levelRange.Min, levelRange.Max + 1);
            }

        }
    }

    [Serializable]
    public class StatDoubleFloatModifier : StatSingleFloatModifier, IStatModifier<float, float>
    {
        public float Value2 { get; set; }
    }
}
