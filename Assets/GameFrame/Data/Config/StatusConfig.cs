using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Data.Config
{
    public struct ModifierEntry
    {
        public string ModifierID;
        public int Value;
    }

    public class StatusConfig
    {
        [ShowInInspector] public string ID { get; set; }
        [ShowInInspector] public string Name { get; set; }
        [ShowInInspector] public string Description { get; set; }
        [ShowInInspector] public int Duration { get; set; }
        [ShowInInspector] public List<ModifierEntry> ModifierEntries { get; set; }
        [ShowInInspector] public string Icon { get; set; }
    }
}
