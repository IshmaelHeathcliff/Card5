using Data.Config;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(menuName = "Tools/Modifier Preset Editor", fileName = "ModifierPresetEditor")]
    public class ModifierPresetEditor : DataPresetEditor<ModifierConfig>
    {
    }
}
