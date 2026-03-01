using Data.Config;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(menuName = "Tools/Drop Preset Editor", fileName = "DropPresetEditor")]
    public class DropPresetEditor : DataPresetEditor<DropConfig>
    {
    }
}
