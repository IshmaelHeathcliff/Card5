using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [CreateAssetMenu(fileName = "NewCard", menuName = "Card5/Card")]
    public class CardData : SerializedScriptableObject
    {
        [SerializeField] string _cardId;
        [SerializeField] string _cardName;
        [SerializeField, TextArea] string _description;
        [SerializeField, MinValue(0)] int _energyCost;
        [SerializeField] Sprite _artwork;
        [SerializeField] List<CardEffectSO> _effects = new List<CardEffectSO>();

        public string CardId => _cardId;
        public string CardName => _cardName;
        public string Description => _description;
        public int EnergyCost => _energyCost;
        public Sprite Artwork => _artwork;
        public IReadOnlyList<CardEffectSO> Effects => _effects;

        void OnValidate()
        {
            if (string.IsNullOrEmpty(_cardId))
                _cardId = name;
        }
    }
}
