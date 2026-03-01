using System.Collections.Generic;
using System.Text;
using Data.Config;
using Gameplay.Modifier;
using UnityEngine;

namespace Gameplay.Stat
{
    public class Stats : IStatModifierFactory
    {
        protected Dictionary<string, IStat> InternalStats = new();

        public string FactoryID { get; set; }

        public static StringBuilder GenerateStatInfo(IStat stat, int indent = 0)
        {
            var info = new StringBuilder();
            info.Append($"{new string(' ', indent * 2)}");
            info.Append($"{stat.Name}: {FormatStatValue(stat.Value)}\n");
            info.Append($"{new string(' ', indent * 2)}");
            info.Append($"  {stat.Name}基础值: {FormatStatValue(stat.BaseValue)}\n");
            info.Append($"{new string(' ', indent * 2)}");
            info.Append($"  {stat.Name}附加值: {FormatStatValue(stat.AddedValue)}\n");
            info.Append($"{new string(' ', indent * 2)}");
            info.Append($"  {stat.Name}固定值: {FormatStatValue(stat.FixedValue)}\n");
            info.Append($"{new string(' ', indent * 2)}");
            info.Append($"  {stat.Name}提高: {(int)stat.Increase}%\n");
            info.Append($"{new string(' ', indent * 2)}");
            info.Append($"  {stat.Name}总增: {(int)((stat.More - 1) * 100)}%\n");
            return info;
        }

        public static string FormatStatValue(float value)
        {
            // 如果是整数或者很接近整数，显示为整数
            if (Mathf.Abs(value - Mathf.Round(value)) < 0.01f)
            {
                return ((int)Mathf.Round(value)).ToString();
            }
            // 否则显示两位小数
            else
            {
                return value.ToString("F2");
            }
        }

        public void AddStat(IStat stat)
        {
            InternalStats[stat.ID] = stat;
        }


        public IEnumerable<IStat> GetAllStats()
        {
            return InternalStats.Values;
        }

        public IStat GetStat(string statName)
        {
            if (InternalStats.TryGetValue(statName, out IStat stat))
            {
                return stat;
            }
            else
            {
                Debug.LogError("未找到Stat: " + statName);
                return null;
            }
        }

        public IConsumableStat GetConsumableStat(string statName)
        {
            IStat stat = GetStat(statName);

            if (stat == null)
            {
                return null;
            }

            if (stat is IConsumableStat consumableStat)
            {
                return consumableStat;
            }
            else
            {
                Debug.LogError($"Stat {statName} is not a consumable stat");
                return null;
            }
        }

        public IKeywordStat GetKeywordStat(string statName)
        {
            IStat stat = GetStat(statName);

            if (stat == null)
            {
                return null;
            }

            if (stat is IKeywordStat keywordStat)
            {
                return keywordStat;
            }
            else
            {
                Debug.LogError($"Stat {statName} is not a keyword stat");
                return null;
            }
        }

        public IStat GetStat(StatModifierConfig modifierConfig)
        {
            return GetStat(modifierConfig.StatName);
        }

        #region StatModifier

        public IModifier CreateModifier(ModifierConfig modifierConfig)
        {
            if (modifierConfig is not StatModifierConfig config)
            {
                Debug.LogError($"modifierConfig {modifierConfig.ModifierID} is not a stat modifier");
                return null;
            }

            return CreateModifier(config);
        }

        public IStatModifier CreateModifier(StatModifierConfig modifierConfig, int value)
        {
            IStat stat = GetStat(modifierConfig.StatName);
            if (stat != null)
            {
                var modifier = new StatSingleFloatModifier(modifierConfig, stat, value)
                {
                    FactoryID = FactoryID
                };
                return modifier;
            }
            else
            {
                Debug.LogError("modifier stat name is not valid: " + modifierConfig.StatName);
                return null;
            }
        }

        public IStatModifier CreateModifier(StatModifierConfig modifierConfig)
        {
            IStat stat = GetStat(modifierConfig.StatName);
            if (stat != null)
            {
                var modifier = new StatSingleFloatModifier(modifierConfig, stat)
                {
                    FactoryID = FactoryID
                };
                if (modifier != null)
                {
                    modifier.RandomizeLevel();
                    modifier.RandomizeValue();
                }

                return modifier;
            }
            else
            {
                Debug.LogError("modifier stat name is not valid: " + modifierConfig.StatName);
                return null;
            }
        }

        #endregion
    }
}
