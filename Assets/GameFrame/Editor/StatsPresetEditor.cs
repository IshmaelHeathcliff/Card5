using Data.Config;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(menuName = "Tools/Stats Preset Editor", fileName = "StatsPresetEditor")]
    public class StatsPresetEditor : DataPresetEditor<StatConfig>
    {
    }
}
