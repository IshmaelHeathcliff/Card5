using Sirenix.OdinInspector;

namespace Data.Config
{
    public class DropConfig
    {
        [ShowInInspector] public string DropID { get; set; }
        [ShowInInspector] public string Name { get; set; }
        [ShowInInspector] public string DropPrefab { get; set; }
    }
}
