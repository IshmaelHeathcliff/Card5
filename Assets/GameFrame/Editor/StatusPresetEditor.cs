using Data.Config;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(menuName = "Tools/Status Preset Editor", fileName = "StatusPresetEditor")]
    public class StatusPresetEditor : DataPresetEditor<StatusConfig>
    {
    }
}
