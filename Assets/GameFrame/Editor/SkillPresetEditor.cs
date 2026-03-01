using Data.Config;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(menuName = "Tools/Skill Preset Editor", fileName = "SkillPresetEditor")]
    public class SkillPresetEditor : DataPresetEditor<SkillConfig>
    {
    }
}
