using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Data.Config
{
    public enum StatType
    {
        Normal,
        Consumable,
        Keyword,
    }

    public class StatConfig
    {
        [ShowInInspector] public string ID { get; set; }
        [ShowInInspector] public string Name { get; set; }
        [ShowInInspector] public StatType Type { get; set; }
        [ShowInInspector] public string Description { get; set; }
    }
}
