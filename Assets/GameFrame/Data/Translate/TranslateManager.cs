using System.Collections.Generic;
using Data.Config;
using Data.SaveLoad;
using UnityEngine;

namespace Data.Translate
{
    public static class TranslateManager
    {
        const string PresetPath = "Preset";
        const string PresetName = "Translate.json";

        static bool s_isLoaded = false;

        static readonly Dictionary<string, TranslateEntryConfig> Dict = new();

        static void Load()
        {
            List<TranslateEntryConfig> configs = SaveLoadManager.Load<List<TranslateEntryConfig>>(PresetName, PresetPath);
            foreach (TranslateEntryConfig config in configs)
            {
                Dict[config.ID] = config;
            }

            s_isLoaded = true;
        }

        public static string GetName(string id)
        {
            if (!s_isLoaded)
            {
                Load();
            }

            if (Dict.TryGetValue(id, out TranslateEntryConfig config))
            {
                return config.Name;
            }

            Debug.Log($"未翻译：{id}");
            return id;
        }

        public static string GetDescription(string id)
        {
            if (!s_isLoaded)
            {
                Load();
            }

            if (Dict.TryGetValue(id, out TranslateEntryConfig config))
            {
                return config.Description;
            }

            Debug.Log($"未翻译：{id}");
            return id;
        }
    }
}
