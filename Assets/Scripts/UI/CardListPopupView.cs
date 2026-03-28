using System.Collections.Generic;
using Card5.Gameplay.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 通用卡牌列表弹窗：可供抽牌堆和弃牌堆共用，显示卡牌名称、费用、描述，支持滚动。
    /// </summary>
    public class CardListPopupView : MonoBehaviour, IController
    {
        [Title("标题与关闭")]
        [SerializeField] TMPro.TextMeshProUGUI _titleText;
        [SerializeField] Button _closeButton;

        [Title("列表")]
        [SerializeField, Required] Transform _contentContainer;
        [SerializeField, Required] CardListEntryView _entryPrefab;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Awake()
        {
            gameObject.SetActive(false);
        }

        void Start()
        {
            _closeButton?.onClick.AddListener(Hide);
        }

        void OnEnable()
        {
            this.RegisterEvent<BattleEndedEvent>(_ => Hide()).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnDestroy()
        {
            _closeButton?.onClick.RemoveListener(Hide);
        }

        public void Show(string title, IReadOnlyList<CardData> cards)
        {
            ClearEntries();

            if (_titleText != null)
                _titleText.text = $"{title}（{cards.Count} 张）";

            foreach (var card in cards)
            {
                var entry = Instantiate(_entryPrefab, _contentContainer);
                entry.Setup(card);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            ClearEntries();
            gameObject.SetActive(false);
        }

        void ClearEntries()
        {
            foreach (Transform child in _contentContainer)
                Destroy(child.gameObject);
        }
    }
}
