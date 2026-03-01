using System;
using Gameplay.Character;
using Gameplay.Skill;
using UnityEngine;

public class ValueCounter
{
    public string ID { get; set; }
    public ICharacterModel Model { get; set; }
    public int Value { get; set; } = 0;
    public EasyEvent<CountChangedEvent> ValueChanged { get; set; } = new();

    public ValueCounter(string id, ICharacterModel model)
    {
        ID = id;
        Model = model;
    }

    public void IncrementCount(int amount)
    {
        Value += amount;
        ValueChanged?.Trigger(new CountChangedEvent(ID, Value, Model));
    }

    public void DecrementCount(int amount)
    {
        Value -= amount;
        ValueChanged?.Trigger(new CountChangedEvent(ID, Value, Model));
    }

}

public struct CountChangedEvent : IReleaseEvent
{
    public string ID { get; set; }
    public int Value { get; set; }
    public ICharacterModel Model { get; set; }

    public CountChangedEvent(string id, int value, ICharacterModel model)
    {
        ID = id;
        Value = value;
        Model = model;
    }
}

public class CountSystem : AbstractSystem
{
    public int GetCount(string ID, ICharacterModel model)
    {
        if (!model.CountValues.ContainsKey(ID))
        {
            model.CountValues.Add(ID, new ValueCounter(ID, model));
        }

        return model.CountValues[ID].Value;
    }

    public IUnRegister Register(string ID, ICharacterModel model, Action<CountChangedEvent> onValueChanged)
    {
        if (!model.CountValues.ContainsKey(ID))
        {
            model.CountValues.Add(ID, new ValueCounter(ID, model));
        }

        return model.CountValues[ID].ValueChanged.Register(onValueChanged);
    }

    public void Unregister(string ID, ICharacterModel model, Action<CountChangedEvent> onValueChanged)
    {
        if (model.CountValues.ContainsKey(ID))
        {
            model.CountValues[ID].ValueChanged.UnRegister(onValueChanged);
        }
    }

    public void IncrementKillCount(ICharacterModel model, int amount)
    {
        IncrementCount("KillCount", model, amount);
    }

    public void IncrementCount(string ID, ICharacterModel model, int amount)
    {
        if (!model.CountValues.ContainsKey(ID))
        {
            model.CountValues.Add(ID, new ValueCounter(ID, model));
        }

        model.CountValues[ID].IncrementCount(amount);
        // Debug.Log($"IncrementCount: {ID} {amount}");
    }

    protected override void OnInit()
    {
    }

}
