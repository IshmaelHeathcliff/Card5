using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Card5
{
    [Serializable]
    public abstract class CardEffect
    {
        [SerializeField, LabelText("补充描述"), TextArea(2, 4)] string _description;

        public string Description => _description;

        public abstract void Execute(BattleContext context);

        public virtual string GetDescription() => _description;
    }
}
