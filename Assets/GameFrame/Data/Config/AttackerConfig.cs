using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Data.Config
{
    public class AttackerConfig
    {
        [ShowInInspector] public string AttackerID { get; set; }
        [ShowInInspector] public string Name { get; set; }
        [ShowInInspector] public string Address { get; set; }
        [ShowInInspector] public List<string> Keywords { get; set; }
    }
}
