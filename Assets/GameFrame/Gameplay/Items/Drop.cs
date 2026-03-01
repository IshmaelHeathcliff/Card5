using Gameplay.Character;
using Gameplay.Damage;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Gameplay.Items
{
    public class Drop : MonoBehaviour, IController
    {
        public string DropID;

        public IArchitecture GetArchitecture()
        {
            return GameFrame.Interface;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                this.GetSystem<ResourceSystem>().AcquireResource("Coin", 1, other.GetComponent<IDamageable>().CharacterController.CharacterModel as IHasResources);
                Addressables.ReleaseInstance(gameObject);
            }
        }
    }
}
