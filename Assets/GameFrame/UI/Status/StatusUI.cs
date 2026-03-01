using Gameplay.Status;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Data;


namespace UI
{
    public class StatusUI : MonoBehaviour
    {
        [SerializeField] Slider _slider;
        [SerializeField] TextMeshProUGUI _info;
        [SerializeField] TextMeshProUGUI _count;
        [SerializeField] Image _icon;

        AsyncOperationHandle<Sprite> _iconHandle;

        public void SetTime(float timeLeft, float duration)
        {
            _slider.value = 1 - timeLeft / duration;
        }

        public void SetCount(int count)
        {
            _count.text = $"{count}";
        }

        public void SetInfo(string statusName, string description)
        {
            _info.text = $"{statusName}\n<size=60%>{description}";
        }

        public void EnableInfo()
        {
            _info.gameObject.SetActive(true);
        }

        public void DisableInfo()
        {
            _info.gameObject.SetActive(false);
        }

        public async UniTaskVoid SetIcon(string iconPath)
        {
            AssetsManager.Release(_iconHandle);
            _iconHandle = Addressables.LoadAssetAsync<Sprite>(iconPath);
            _icon.sprite = await _iconHandle;
        }

        public void InitStatusUI(IStatus status)
        {
            if (status is IStatusWithTime bt)
            {
                _slider.gameObject.SetActive(true);
                SetTime(bt.TimeLeft, bt.Duration);
            }

            if (status is IStatusWithCount bc)
            {
                _count.gameObject.SetActive(true);
                SetCount(bc.Count);
            }

            SetInfo(status.GetName(), status.GetDescription());
            SetIcon(status.GetIconPath()).Forget();
        }


        void OnEnable()
        {
            _slider.gameObject.SetActive(false);
            _count.gameObject.SetActive(false);
            _info.gameObject.SetActive(false);
        }

        void OnDisable()
        {
            AssetsManager.Release(_iconHandle);
        }

        void OnValidate()
        {
            _slider = GetComponentInChildren<Slider>(true);
            _info = transform.Find("Info").GetComponent<TextMeshProUGUI>();
            _count = transform.Find("Count").GetComponent<TextMeshProUGUI>();
            _icon = transform.Find("Icon").GetComponent<Image>();
        }
    }
}
