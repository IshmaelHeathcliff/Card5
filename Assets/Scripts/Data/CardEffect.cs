using System;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public abstract class CardEffect
    {
        [SerializeField, TextArea] string _description;

        public string Description => _description;

        public abstract void Execute(BattleContext context);

        public virtual string GetDescription() => _description;
    }
}
