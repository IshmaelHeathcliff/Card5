using System;
using System.Collections.Generic;
using System.Linq;
using Data.Config;
using Gameplay.Modifier;

namespace Gameplay.Status
{
    public interface IStatus
    {
        string GetName();
        string GetID();
        string GetDescription();
        string GetIconPath();
        void Enable();
        void Disable();
    }

    public interface IStatusWithTime : IStatus
    {
        float Duration { get; set; }
        float TimeLeft { get; }
        void ResetTime();
        void DecreaseTime(float time);
    }

    public interface IStatusWithCount : IStatus
    {
        int Count { get; set; }
        int MaxCount { get; set; }
    }

    [Serializable]
    public class Status : IStatus
    {
        StatusConfig _config;

        List<IModifier> _modifiers;

        public Status(StatusConfig config, IEnumerable<IModifier> entries)
        {
            _config = config;
            _modifiers = entries.ToList();
            Enable();
        }

        public string GetName()
        {
            return _config.Name;
        }

        public string GetID()
        {
            return _config.ID;
        }

        public string GetDescription()
        {
            return _config.Description;
        }

        public string GetIconPath()
        {
            return _config.Icon;
        }

        public void Enable()
        {
            foreach (IModifier modifier in _modifiers)
            {
                modifier.Register();
            }
        }

        public void Disable()
        {
            foreach (IModifier modifier in _modifiers)
            {
                modifier.Unregister();
            }
        }
    }

    public class StatusWithTime : Status, IStatusWithTime
    {
        public float Duration { get; set; }
        public float TimeLeft { get; private set; }
        public StatusWithTime(StatusConfig config, IEnumerable<IModifier> entries, float time) : base(config, entries)
        {
            Duration = time;
            TimeLeft = time;
            Enable();
        }

        public void ResetTime()
        {
            TimeLeft = Duration;
        }

        public void DecreaseTime(float time)
        {
            TimeLeft -= time;
            if (TimeLeft <= 0)
            {
                Disable();
            }
        }
    }

    public class StatusWithCount : Status, IStatusWithCount
    {
        public int Count { get; set; }
        public int MaxCount { get; set; }
        public StatusWithCount(StatusConfig config, IEnumerable<IModifier> entries, int maxCount) : base(config, entries)
        {
            Count = 1;
            MaxCount = maxCount;
            Enable();
        }
    }

    public class StatusWithTimeAndCount : StatusWithTime, IStatusWithCount
    {
        public int Count { get; set; }
        public int MaxCount { get; set; }
        public StatusWithTimeAndCount(StatusConfig config, IEnumerable<IModifier> entries, float time, int maxCount) : base(config, entries, time)
        {
            Count = 1;
            MaxCount = maxCount;
            Enable();
        }
    }
}
