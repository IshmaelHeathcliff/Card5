using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Card5
{
    /// <summary>
    /// 印记配置数据：定义印记名称、描述、持续回合数和触发的效果列表。
    /// </summary>
    [CreateAssetMenu(fileName = "NewMark", menuName = "Card5/Mark")]
    public class MarkData : SerializedScriptableObject
    {
        [SerializeField] string _markId;
        [SerializeField] string _markName;
        [SerializeField, TextArea] string _description;
        [SerializeField] Sprite _icon;

        [SerializeField, Tooltip("持续回合数，-1 表示永久")]
        int _duration = 3;

        [SerializeField, Tooltip("印记效果触发时机")]
        MarkTrigger _trigger = MarkTrigger.AfterCardEffects;

        [OdinSerialize, LabelText("印记效果"), ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true), PolymorphicDrawerSettings(ShowBaseType = false)]
        List<CardEffect> _effects = new List<CardEffect>();

        public string MarkId => _markId;
        public string MarkName => _markName;
        public string Description => _description;
        public Sprite Icon => _icon;

        /// <summary>持续回合数，-1 表示永久不消失</summary>
        public int Duration => _duration;

        public MarkTrigger Trigger => _trigger;
        public IReadOnlyList<CardEffect> Effects => _effects;

        void OnValidate()
        {
            if (string.IsNullOrEmpty(_markId))
                _markId = name;
        }

        public string GetDescription()
        {
            string durationStr = _duration < 0 ? "永久" : $"{_duration} 回合";
            return $"[{_markName}]（{durationStr}）{_description}";
        }
    }

    public enum MarkTrigger
    {
        /// <summary>在该槽/卡的主效果之前触发</summary>
        BeforeCardEffects,
        /// <summary>在该槽/卡的主效果之后触发</summary>
        AfterCardEffects
    }
}
