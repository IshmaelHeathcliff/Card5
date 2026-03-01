
using Sirenix.OdinInspector;

namespace Data.Config
{
    public class TranslateEntryConfig
    {
        [ShowInInspector] public string ID { get; set; }
        [ShowInInspector] public string Name { get; set; }
        [ShowInInspector] public string Description { get; set; }
    }
}