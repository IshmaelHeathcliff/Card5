using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Character
{
    public abstract class CharactersModel<TModel> : AbstractModel where TModel : ICharacterModel, new()
    {
        string _currentID;

        //* 实时调用Current是更安全的
        public TModel Current
        {
            get
            {
                if (TryGetModel(_currentID, out TModel model))
                {
                    return model;
                }

                Debug.LogError($"Current model {_currentID} does not exist;");
                return default;
            }

            set => _currentID = value.ID;
        }

        readonly protected Dictionary<string, TModel> Models = new();
        public bool TryGetModel(string id, out TModel model)
        {
            if (Models.TryGetValue(id, out model))
            {
                return true;
            }

            return false;
        }

        public TModel AddModel(string id)
        {
            if (!Models.TryGetValue(id, out TModel model))
            {
                model = new TModel
                {
                    ID = id
                };

                Models.Add(id, model);
            }

            return model;
        }

        public void RemoveModel(string id)
        {
            if (TryGetModel(id, out TModel model))
            {
                model.SkillsInSlot.Clear();
                model.SkillsReleased.Clear();
                model.StatusContainer.Clear();
                model.Controller = null;
                Models.Remove(id);
            }
        }

        public Dictionary<string, Transform> GetTransforms()
        {
            return Models.ToDictionary(pair => pair.Key, pair => pair.Value.Transform);
        }
    }
}