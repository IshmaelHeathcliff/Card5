using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "NewDeckPreset", menuName = "Card5/Deck Preset")]
    public class DeckPresetData : ScriptableObject
    {
        [SerializeField] string _deckName;
        [SerializeField, ListDrawerSettings(ShowPaging = true)] List<CardEntry> _cards = new List<CardEntry>();

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
        [HorizontalGroup, HideLabel] public CardData Card;
        [HorizontalGroup(Width = 50), HideLabel, MinValue(1), MaxValue(10)] public int Count = 1;
    }
}
