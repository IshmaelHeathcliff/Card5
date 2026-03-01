using System;
using Gameplay.Character;
using Gameplay.Character.Player;
using UnityEngine;

namespace Gameplay.Items
{
    public class ResourceSystem : AbstractSystem
    {
        CountSystem _countSystem;


        public int GetResourceCount(string id, IHasResources model)
        {
            return model.Resources.GetResourceCount(id);
        }

        public void AcquireResource(string id, int amount, IHasResources model)
        {
            model.Resources.AddResourceCount(id, amount);
        }

        public void ConsumeResource(string id, int amount, IHasResources model)
        {
            model.Resources.AddResourceCount(id, -amount);

            if (model is not ICharacterModel characterModel)
            {
                return;
            }

            if (id == ResourceType.Wood.ToString())
            {
                _countSystem.IncrementCount("WoodConsumed", characterModel, amount);
            }
            else if (id == ResourceType.Coin.ToString())
            {
                _countSystem.IncrementCount("CoinConsumed", characterModel, amount);
            }
        }

        public int GetResourceCount(ResourceType type, IHasResources model)
        {
            return GetResourceCount(type.ToString(), model);
        }

        public void AcquireResource(ResourceType type, int amount, IHasResources model)
        {
            AcquireResource(type.ToString(), amount, model);
        }


        public void ConsumeResource(ResourceType type, int amount, IHasResources model)
        {
            ConsumeResource(type.ToString(), amount, model);
        }


        public IUnRegister Register(ResourceType type, Action<int> onValueChanged, IHasResources model)
        {
            return model.Resources.Register(type.ToString(), onValueChanged);
        }

        public IUnRegister RegisterWithInitValue(ResourceType type, Action<int> onValueChanged, IHasResources model)
        {
            return model.Resources.RegisterWithInitValue(type.ToString(), onValueChanged);
        }

        public void Unregister(ResourceType type, Action<int> onValueChanged, IHasResources model)
        {
            model.Resources.UnRegister(type.ToString(), onValueChanged);
        }


        protected override void OnInit()
        {
            _countSystem = this.GetSystem<CountSystem>();
        }
    }
}
