using UnityEngine;

namespace Gameplay.Character
{
    public interface ICharacterControlled : ICanInit
    {
        ICharacterController CharacterController { get; set; }
    }

    public abstract class CharacterControlled : MonoBehaviour, ICharacterControlled
    {
        public bool Initialized { get; set; }
        public ICharacterController CharacterController { get; set; }

        public void Init()
        {
            if (Initialized)
            {
                return;
            }

            if (CharacterController == null)
            {
                CharacterController = GetComponentInParent<ICharacterController>();
            }

            if (!CharacterController.Initialized)
            {
                CharacterController.Init();
            }

            OnInit();
            Initialized = true;
        }

        public void Deinit()
        {
            if (!Initialized)
            {
                return;
            }

            OnDeinit();
            Initialized = false;
        }

        protected abstract void OnInit();
        protected abstract void OnDeinit();

        protected virtual void Awake()
        {
            if (!Initialized)
            {
                Init();
            }
        }
    }
}
