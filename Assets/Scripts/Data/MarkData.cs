using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Card5
{
    /// <summary>
    /// 印记配置数据：定义印记名称、描述、持续回合数和触发的效果列表。
    /// </summary>
    [HideMonoScript]
    [CreateAssetMenu(fileName = "NewMark", menuName = "Card5/Mark")]
    public class MarkData : SerializedScriptableObject
    {
        [BoxGroup("基础信息文本"), SerializeField, LabelText("印记ID")] string _markId;
        [BoxGroup("基础信息文本"), SerializeField, LabelText("印记名称")] string _markName;
        [BoxGroup("基础信息文本"), SerializeField, LabelText("描述"), TextArea(3, 6)] string _description;
        [BoxGroup("基础信息视觉"), SerializeField, LabelText("图标"), PreviewField(90, ObjectFieldAlignment.Center)] Sprite _icon;

        [BoxGroup("规则"), SerializeField, LabelText("持续回合数"), Tooltip("持续回合数，-1 表示永久"), MinValue(-1)]
        int _duration = 3;

        [BoxGroup("规则"), SerializeField, LabelText("触发时机"), Tooltip("印记效果触发时机")]
        MarkTrigger _trigger = MarkTrigger.AfterCardEffects;

        [BoxGroup("效果配置")]
        [OdinSerialize, LabelText("印记效果"), ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true, DraggableItems = true), PolymorphicDrawerSettings(ShowBaseType = false)]
        List<CardEffect> _effects = new List<CardEffect>();

        public string MarkId => _markId;
        public string MarkName => _markName;
        public string Description => _description;
        public Sprite Icon => _icon;

        /// <summary>持续回合数，-1 表示永久不消失</summary>
        public int Duration => _duration;

        public MarkTrigger Trigger => _trigger;
        public IReadOnlyList<CardEffect> Effects => _effects;

        [BoxGroup("规则"), ShowInInspector, ReadOnly, LabelText("持续时间说明")]
        string DurationDescription => _duration < 0 ? "永久" : $"{_duration} 回合";

        [BoxGroup("规则"), ShowInInspector, ReadOnly, MultiLineProperty(4), LabelText("完整描述")]
        string InspectorDescription => GetDescription();

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
        [InspectorName("卡牌效果前")]
        BeforeCardEffects,
        /// <summary>在该槽/卡的主效果之后触发</summary>
        [InspectorName("卡牌效果后")]
        AfterCardEffects
    }
}
