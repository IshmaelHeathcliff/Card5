using Data.Config;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(menuName = "Tools/Translate Preset Editor", fileName = "TranslatePresetEditor")]
    public class TranslatePresetEditor : DataPresetEditor<TranslateEntryConfig>
    {
    }
}