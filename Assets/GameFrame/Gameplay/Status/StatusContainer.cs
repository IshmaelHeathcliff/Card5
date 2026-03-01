using System.Collections.Generic;
using Sirenix.Utilities;

namespace Gameplay.Status
{
    public interface IStatusContainer
    {
        EasyEvent<IStatus> OnStatusAdded { get; }
        EasyEvent<string> OnStatusRemoved { get; }
        EasyEvent<IStatusWithTime> OnStatusTimeChanged { get; }
        EasyEvent<IStatusWithCount> OnStatusCountChanged { get; }
        void AddStatus(IStatus status);
        void RemoveStatus(string id);
        void RemoveStatus(IStatus status);
        bool HasStatus(string id);
        bool HasStatus(IStatus status);
        void ResetStatusTime(float time);
        void DecreaseStatusTime(float time);
        void Clear();
    }


    public class StatusContainer : IStatusContainer
    {
        readonly Dictionary<string, IStatus> _statusDic = new();
        public EasyEvent<IStatus> OnStatusAdded { get; } = new();
        public EasyEvent<string> OnStatusRemoved { get; } = new();
        public EasyEvent<IStatusWithTime> OnStatusTimeChanged { get; } = new();
        public EasyEvent<IStatusWithCount> OnStatusCountChanged { get; } = new();

        public void AddStatus(IStatus status)
        {
            if (_statusDic.ContainsKey(status.GetID()))
            {
                // 如果状态已经存在，则更新状态而不是添加状态
                if (status is not IStatusWithCount)
                {
                    RemoveStatus(status);
                }
                else
                {
                    // TODO：处理Status的叠层
                    return;
                }
            }

            _statusDic.Add(status.GetID(), status);
            status.Enable();
            OnStatusAdded?.Trigger(status);

        }

        public bool HasStatus(string id)
        {
            return _statusDic.ContainsKey(id);
        }

        public bool HasStatus(IStatus status)
        {
            return HasStatus(status.GetID());
        }

        public void RemoveStatus(IStatus status)
        {
            RemoveStatus(status.GetID());
        }

        public void RemoveStatus(string id)
        {
            if (HasStatus(id))
            {
                _statusDic[id].Disable();
                _statusDic.Remove(id);
                OnStatusRemoved?.Trigger(id);
            }
        }

        public void ResetStatusTime(float time)
        {
            foreach (IStatus status in _statusDic.Values)
            {
                if (status is IStatusWithTime bt)
                {
                    bt.ResetTime();
                    OnStatusTimeChanged?.Trigger(bt);
                }
            }
        }

        public void DecreaseStatusTime(float time)
        {

            List<IStatus> statusList = new();
            _statusDic.Values.ForEach(status => statusList.Add(status));
            foreach (IStatus status in statusList)
            {
                if (status is not IStatusWithTime st)
                {
                    continue;
                }

                st.DecreaseTime(time);
                OnStatusTimeChanged?.Trigger(st);
                if (st.TimeLeft <= 0)
                {
                    RemoveStatus(status);
                }
            }
        }

        public void ChangeStatusCount(string id, int count)
        {
            if (_statusDic.TryGetValue(id, out IStatus status))
            {
                if (status is IStatusWithCount bc)
                {
                    bc.Count = count;
                    OnStatusCountChanged?.Trigger(bc);
                }
            }
        }

        public void Clear()
        {
            foreach (IStatus status in _statusDic.Values)
            {
                status.Disable();
            }
            _statusDic.Clear();
        }

    }
}
