using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Stat
{
    public interface IKeywordStat : IStat
    {
        List<string> Keywords { get; }
        List<string> KeywordsToQuery { get; set; }

        float GetValueByKeywords();

        /// <summary>
        /// 当Modifier的Keywords都包含在KeywordsToQuery中时，该Modifier才会被计算在内
        /// </summary>
        /// <param name="keywordsToQuery"></param>
        /// <returns></returns>
        float GetValueByKeywords(List<string> keywordsToQuery);

        /// <summary>
        /// 当Modifier的Keywords都包含在KeywordsToQuery中时，该Modifier才会被计算在内
        /// </summary>
        /// <param name="baseValue"></param>
        /// <param name="keywordsToQuery"></param>
        /// <param name="addedMultiplier"></param>
        /// <returns></returns>
        float GetValueByKeywords(float baseValue, List<string> keywordsToQuery, float addedMultiplier = 1);
    }


    public class KeywordStat : Stat, IKeywordStat
    {
        // 预定义关键词
        public List<string> Keywords { get; }

        // 实际查询关键词
        public List<string> KeywordsToQuery { get; set; } = new();

        public KeywordStat(string id, string name, List<string> keywords = null) : base(id, name)
        {
            if (keywords != null)
            {
                Keywords = keywords;
            }
        }

        public override float Value => GetValueByKeywords();

        // 筛选出所有关键词都包含在KeywordsToQuery中的Modifier，然后求和它们的值
        // x.Keywords.All(KeywordsToQuery.Contains) 确保Modifier的所有关键词都在查询关键词列表中
        // 这实现了严格的关键词匹配：只有当Modifier的所有关键词都匹配时，该Modifier才会被计算在内
        // 当Modifier的Keywords为空时，该Modifier也会被计算在内
        public override float AddedValue => AddedValueModifiers.Values.Where(x => x.Keywords.All(KeywordsToQuery.Contains)).Sum(x => x.Value);
        public override float Increase => IncreaseModifiers.Values.Where(x => x.Keywords.All(KeywordsToQuery.Contains)).Sum(x => x.Value);
        public override float More => MoreModifiers.Values.Where(x => x.Keywords.All(KeywordsToQuery.Contains)).Aggregate(1f, (acc, mod) => acc * ((float)mod.Value / 100 + 1));
        public override float FixedValue => FixedValueModifiers.Values.Where(x => x.Keywords.All(KeywordsToQuery.Contains)).Sum(x => x.Value);

        public float GetValueByKeywords()
        {
            if (Keywords != null)
            {
                KeywordsToQuery = Keywords;
                return Calculate();
            }

            return GetValue();
        }


        public float GetValueByKeywords(List<string> keywordsToQuery)
        {
            KeywordsToQuery = keywordsToQuery;
            return Calculate();
        }

        public float GetValueByKeywords(float baseValue, List<string> keywordsToQuery, float addedMultiplier = 1)
        {
            KeywordsToQuery = keywordsToQuery;
            BaseValue = baseValue;
            return Calculate(addedMultiplier);
        }
    }
}
