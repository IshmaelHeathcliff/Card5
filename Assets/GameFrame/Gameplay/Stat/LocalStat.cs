using System;
using System.Collections.Generic;
using Gameplay.Modifier;
using UnityEngine;

namespace Gameplay.Stat
{

    public class LocalStat : IStat
    {
        readonly IStat _global;
        float _baseValue;

        protected IStat Local;
        protected EasyEvent<float> OnValueChanged = new();

        public string ID { get; }
        public string Name { get; }
        public virtual float Value => GetValue();


        public float BaseValue
        {
            get => _baseValue;
            set
            {
                _baseValue = value;
                OnValueChanged.Trigger(Value);
            }
        }

        public virtual float AddedValue => Local.AddedValue + _global.AddedValue;
        public virtual float FixedValue => Local.FixedValue + _global.FixedValue;
        public virtual float Increase => Local.Increase + _global.Increase;
        public virtual float More => Local.More * _global.More;

        public LocalStat(IStat localStat, IStat globalStat)
        {
            ID = localStat.ID;
            Name = localStat.Name;
            Local = localStat;
            _global = globalStat;

            Local.Register(_ => OnValueChanged.Trigger(Value));
            _global.Register(_ => OnValueChanged.Trigger(Value));
        }

        protected float Calculate(float addedMultiplier = 1)
        {
            return (BaseValue + AddedValue * addedMultiplier) * (1 + Increase / 100f) * More + FixedValue;
        }

        public float GetValue()
        {
            return Calculate();
        }

        public float GetValue(float baseValue, float addedMultiplier = 1)
        {
            BaseValue = baseValue;
            return Calculate(addedMultiplier);
        }

        public void AddAddedValueModifier(string key, IStatModifier<float> mod) => Local.AddAddedValueModifier(key, mod);
        public void AddFixedValueModifier(string key, IStatModifier<float> mod) => Local.AddFixedValueModifier(key, mod);
        public void AddIncreaseModifier(string key, IStatModifier<float> mod) => Local.AddIncreaseModifier(key, mod);
        public void AddMoreModifier(string key, IStatModifier<float> mod) => Local.AddMoreModifier(key, mod);
        public void RemoveAddedValueModifier(string key) => Local.RemoveAddedValueModifier(key);
        public void RemoveFixedValueModifier(string key) => Local.RemoveFixedValueModifier(key);
        public void RemoveIncreaseModifier(string key) => Local.RemoveIncreaseModifier(key);
        public void RemoveMoreModifier(string key) => Local.RemoveMoreModifier(key);

        public IUnRegister Register(Action onValueChanged)
        {
            return Register(v => onValueChanged());
        }

        public IUnRegister Register(Action<float> onValueChanged) => OnValueChanged.Register(onValueChanged);

        public IUnRegister RegisterWithInitValue(Action<float> onValueChanged)
        {
            onValueChanged(Value);
            return Register(onValueChanged);
        }

        public void UnRegister(Action<float> onValueChanged) => OnValueChanged.UnRegister(onValueChanged);
    }

    public class LocalKeywordStat : LocalStat, IKeywordStat
    {
        IKeywordStat _localKeywordStat;
        IKeywordStat _globalKeywordStat;

        public LocalKeywordStat(List<string> keywords, IKeywordStat localStat, IKeywordStat globalStat) : base(localStat, globalStat)
        {
            _localKeywordStat = localStat;
            _globalKeywordStat = globalStat;

            Keywords = keywords;
        }

        public override float Value => GetValueByKeywords();

        public List<string> Keywords { get; protected set; }

        public List<string> KeywordsToQuery
        {
            get => _localKeywordStat.KeywordsToQuery;
            set
            {
                _localKeywordStat.KeywordsToQuery = value;
                _globalKeywordStat.KeywordsToQuery = value;
            }
        }

        public override float AddedValue
        {
            get
            {
                _globalKeywordStat.KeywordsToQuery = _localKeywordStat.KeywordsToQuery;
                return _localKeywordStat.AddedValue + _globalKeywordStat.AddedValue;
            }
        }
        public override float FixedValue
        {
            get
            {
                _globalKeywordStat.KeywordsToQuery = _localKeywordStat.KeywordsToQuery;
                return _localKeywordStat.FixedValue + _globalKeywordStat.FixedValue;
            }
        }
        public override float Increase
        {
            get
            {
                _globalKeywordStat.KeywordsToQuery = _localKeywordStat.KeywordsToQuery;
                return _localKeywordStat.Increase + _globalKeywordStat.Increase;
            }
        }
        public override float More
        {
            get
            {
                _globalKeywordStat.KeywordsToQuery = _localKeywordStat.KeywordsToQuery;
                return _localKeywordStat.More * _globalKeywordStat.More;
            }
        }

        public float GetValueByKeywords()
        {
            if (Keywords != null)
            {
                KeywordsToQuery = Keywords;
                return Calculate();
            }

            Debug.LogError($"Keywords is null for {ID}, return total value");
            return GetValue();
        }

        public float GetValueByKeywords(List<string> keywords)
        {
            _localKeywordStat.KeywordsToQuery = keywords;
            _globalKeywordStat.KeywordsToQuery = keywords;
            return GetValue();
        }

        public float GetValueByKeywords(float baseValue, List<string> keywords, float addedMultiplier = 1)
        {
            BaseValue = baseValue;
            _localKeywordStat.KeywordsToQuery = keywords;
            _globalKeywordStat.KeywordsToQuery = keywords;
            return Calculate(addedMultiplier);
        }
    }

    public class LocalConsumableStat : LocalStat, IConsumableStat, IReadonlyBindableProperty<float, float>
    {
        float _currentValue;
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                value = Mathf.Clamp(value, 0, Value);
                if (value == _currentValue)
                {
                    return;
                }

                _currentValue = value;
                _onValueChanged?.Trigger(_currentValue, Value);
            }
        }

        readonly EasyEvent<float, float> _onValueChanged = new();

        public LocalConsumableStat(IConsumableStat localStat, IConsumableStat globalStat) : base(localStat, globalStat)
        {
            CurrentValue = Value;
        }

        public void ChangeCurrentValue(float value)
        {
            CurrentValue += value;
        }

        public void SetCurrentValue(float value)
        {
            CurrentValue = value;
        }

        public void SetMaxValue()
        {
            CurrentValue = Value;
        }

        public IUnRegister Register(Action<float, float> onValueChanged)
        {
            return _onValueChanged.Register(onValueChanged);
        }

        public IUnRegister RegisterWithInitValue(Action<float, float> onValueChanged)
        {
            onValueChanged(CurrentValue, Value);
            return Register(onValueChanged);
        }

        public void UnRegister(Action<float, float> onValueChanged)
        {
            _onValueChanged.UnRegister(onValueChanged);
        }
    }
}
