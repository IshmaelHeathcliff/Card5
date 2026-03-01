using UnityEngine;

namespace Core.Scene
{
    public class SceneEntrance : MonoBehaviour, IController
    {
        [SerializeField] string _entranceTag;

        void OnEnable()
        {
            this.GetModel<SceneModel>().RegisterEntrance(_entranceTag, this);
        }

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }
    }
}
