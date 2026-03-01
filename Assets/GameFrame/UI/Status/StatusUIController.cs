using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Status;
using UnityEngine;

namespace UI
{
    public interface IStatusUIController
    {
        void AddStatus(IStatus status);
        void RemoveStatus(string id);
        void ChangeStatusTime(IStatusWithTime status);
        void ChangeStatusCount(IStatusWithCount status);
    }

    [RequireComponent(typeof(StatusUIPool))]
    public abstract class StatusUIController : MonoBehaviour, IController, IStatusUIController
    {
        protected IStatusContainer StatusContainer;

        protected abstract void SetStatusContainer();

        readonly Dictionary<string, StatusUI> _statusUIs = new();
        [SerializeField] StatusUIPool _pool;

        async UniTaskVoid AddStatusAsync(IStatus status)
        {
            StatusUI statusUI = await _pool.Allocate();

            if (_statusUIs.ContainsKey(status.GetID()))
            {
                RemoveStatus(status.GetID());
            }

            _statusUIs.Add(status.GetID(), statusUI);
            statusUI.InitStatusUI(status);
        }

        public void AddStatus(IStatus status)
        {
            AddStatusAsync(status).Forget();

        }

        public void RemoveStatus(string id)
        {
            if (_statusUIs.Remove(id, out StatusUI statusUI))
            {
                _pool.Recycle(statusUI);
            }
        }

        public void ChangeStatusTime(IStatusWithTime status)
        {
            if (_statusUIs.TryGetValue(status.GetID(), out StatusUI statusUI))
            {
                statusUI.SetTime(status.TimeLeft, status.Duration);
            }
        }

        public void ChangeStatusCount(IStatusWithCount status)
        {
            if (_statusUIs.TryGetValue(status.GetID(), out StatusUI statusUI))
            {
                statusUI.SetCount(status.Count);
            }
        }

        void OnValidate()
        {
            _pool = GetComponent<StatusUIPool>();
        }

        void Start()
        {
            SetStatusContainer();
            StatusContainer.OnStatusAdded.Register(AddStatus);
            StatusContainer.OnStatusRemoved.Register(RemoveStatus);
            StatusContainer.OnStatusTimeChanged.Register(ChangeStatusTime);
            StatusContainer.OnStatusCountChanged.Register(ChangeStatusCount);
        }

        void FixedUpdate()
        {
            StatusContainer.DecreaseStatusTime(Time.fixedDeltaTime);
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
