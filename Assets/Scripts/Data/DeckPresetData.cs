using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "NewDeckPreset", menuName = "Card5/Deck Preset")]
    public class DeckPresetData : ScriptableObject
    {
        [BoxGroup("基础信息"), SerializeField, LabelText("牌组名称")] string _deckName;

        [BoxGroup("概览"), ShowInInspector, ReadOnly, LabelText("唯一卡牌数")]
        int UniqueCardCount => _cards.Count(entry => entry != null && entry.Card != null);

        [BoxGroup("概览"), ShowInInspector, ReadOnly, LabelText("总卡牌数")]
        int TotalCardCount => _cards.Sum(entry => entry != null ? entry.Count : 0);

        [BoxGroup("卡牌列表")]
        [SerializeField, LabelText("卡牌列表"), ListDrawerSettings(ShowPaging = true, DefaultExpandedState = true, DraggableItems = true)]
        [Searchable]
        List<CardEntry> _cards = new List<CardEntry>();

        public string DeckName => _deckName;
        public IReadOnlyList<CardEntry> Cards => _cards;

        public List<CardData> BuildCardList()
        {
            var result = new List<CardData>();
            foreach (var entry in _cards)
            {
                if (entry.Card == null) continue;
                for (int i = 0; i < entry.Count; i++)
                    result.Add(entry.Card);
            }
            return result;
        }
    }

    [System.Serializable]
    public class CardEntry
    {
        [HorizontalGroup("卡牌行", Width = 90), ShowInInspector, ReadOnly, PreviewField(70, ObjectFieldAlignment.Left), LabelText("预览")]
        Sprite ArtworkPreview => Card != null ? Card.Artwork : null;

        [VerticalGroup("卡牌信息"), LabelText("卡牌")] public CardData Card;
        [VerticalGroup("卡牌信息"), LabelText("数量"), MinValue(1), MaxValue(10)] public int Count = 1;
        [VerticalGroup("卡牌信息"), ShowInInspector, ReadOnly, LabelText("说明")]
        string Summary => $"{Card?.CardName ?? "未配置卡牌"} x{Count}";

        public override string ToString()
        {
            return Summary;
        }
    }
}
