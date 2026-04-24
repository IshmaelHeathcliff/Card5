using System.Collections.Generic;
using Card5.Gameplay.Events;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Card5
{
    /// <summary>
    /// 运行时弹窗管理器：常驻监听业务事件，按需加载弹窗预制体并挂到对应 UI 层级。
    /// </summary>
    public class UIPopupManager : MonoBehaviour, IController
    {
        [SerializeField] Canvas _rootCanvas;
        [SerializeField] AssetReferenceGameObject _battleRewardPopupPrefab;
        [SerializeField] AssetReferenceGameObject _cardListPopupPrefab;

        BattleRewardPopupView _battleRewardPopup;
        CardListPopupView _cardListPopup;
        GameObject _resultPanel;
        Button _resultConfirmButton;
        TMPro.TextMeshProUGUI _resultTitleText;
        TMPro.TextMeshProUGUI _resultMessageText;
        Image _resultButtonImage;

        AsyncOperationHandle<GameObject> _battleRewardPopupHandle;
        AsyncOperationHandle<GameObject> _cardListPopupHandle;

        bool _isBattleRewardPopupLoading;
        bool _isCardListPopupLoading;

        public static UIPopupManager Instance { get; private set; }

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void OnValidate()
        {
            if (_rootCanvas == null)
                _rootCanvas = GetComponentInParent<Canvas>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_rootCanvas == null)
                _rootCanvas = GetComponentInParent<Canvas>();
        }

        void OnEnable()
        {
            this.RegisterEvent<BattleRewardOfferedEvent>(OnBattleRewardOffered).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleRewardOptionClaimedEvent>(OnBattleRewardOptionClaimed).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleRewardCompletedEvent>(_ => HideBattleRewardPopup()).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleStartedEvent>(_ => HideAll()).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<BattleEndedEvent>(OnBattleEnded).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (_resultConfirmButton != null)
                _resultConfirmButton.onClick.RemoveListener(OnResultConfirmClicked);

            ReleasePopup(_battleRewardPopupHandle);
            ReleasePopup(_cardListPopupHandle);
        }

        public async UniTask ShowCardListAsync(string title, IReadOnlyList<CardData> cards)
        {
            CardListPopupView popup = await GetCardListPopupAsync();
            if (popup == null) return;

            var cardSnapshot = new List<CardData>(cards);
            popup.Show(title, cardSnapshot);
        }

        public void HideAll()
        {
            HideBattleRewardPopup();

            if (_cardListPopup != null)
                _cardListPopup.Hide();

            if (_resultPanel != null)
                _resultPanel.SetActive(false);
        }

        void OnBattleRewardOffered(BattleRewardOfferedEvent e)
        {
            ShowBattleRewardPopupAsync(e.Offers).Forget();
        }

        void OnBattleRewardOptionClaimed(BattleRewardOptionClaimedEvent e)
        {
            if (e.RemainingOffers.Count > 0)
                ShowBattleRewardPopupAsync(e.RemainingOffers).Forget();
            else
                HideBattleRewardPopup();
        }

        void OnBattleEnded(BattleEndedEvent e)
        {
            HideBattleRewardPopup();

            if (_cardListPopup != null)
                _cardListPopup.Hide();

            ShowBattleResult(e.PlayerWon);
        }

        async UniTaskVoid ShowBattleRewardPopupAsync(IReadOnlyList<BattleRewardOffer> offers)
        {
            BattleRewardPopupView popup = await GetBattleRewardPopupAsync();
            if (popup == null) return;

            popup.Show(offers);
        }

        async UniTask<BattleRewardPopupView> GetBattleRewardPopupAsync()
        {
            if (_battleRewardPopup != null)
                return _battleRewardPopup;

            if (_isBattleRewardPopupLoading)
            {
                await UniTask.WaitUntil(() => _battleRewardPopup != null || !_isBattleRewardPopupLoading);
                return _battleRewardPopup;
            }

            _isBattleRewardPopupLoading = true;
            try
            {
            _battleRewardPopup = await LoadPopupAsync<BattleRewardPopupView>(_battleRewardPopupPrefab, UILayer.Popup, handle => _battleRewardPopupHandle = handle);
            }
            finally
            {
                _isBattleRewardPopupLoading = false;
            }

            return _battleRewardPopup;
        }

        async UniTask<CardListPopupView> GetCardListPopupAsync()
        {
            if (_cardListPopup != null)
                return _cardListPopup;

            if (_isCardListPopupLoading)
            {
                await UniTask.WaitUntil(() => _cardListPopup != null || !_isCardListPopupLoading);
                return _cardListPopup;
            }

            _isCardListPopupLoading = true;
            try
            {
            _cardListPopup = await LoadPopupAsync<CardListPopupView>(_cardListPopupPrefab, UILayer.Popup, handle => _cardListPopupHandle = handle);
            }
            finally
            {
                _isCardListPopupLoading = false;
            }

            return _cardListPopup;
        }

        async UniTask<T> LoadPopupAsync<T>(AssetReferenceGameObject prefabReference, UILayer layer, System.Action<AsyncOperationHandle<GameObject>> handleSetter)
            where T : Component
        {
            if (prefabReference == null || !prefabReference.RuntimeKeyIsValid())
            {
                Debug.LogError($"[UIPopupManager] 弹窗引用无效: {typeof(T).Name}");
                return null;
            }

            Transform parent = GetLayerParent(layer);
            AsyncOperationHandle<GameObject> handle = prefabReference.InstantiateAsync(parent);
            handleSetter?.Invoke(handle);

            GameObject instance = await handle;
            if (instance == null) return null;

            UILayerManager.MoveToLayer(instance.transform, layer, false);
            T view = instance.GetComponent<T>();
            if (view == null)
            {
                Debug.LogError($"[UIPopupManager] 弹窗预制体缺少组件 {typeof(T).Name}: {prefabReference.RuntimeKey}");
                Addressables.ReleaseInstance(instance);
                return null;
            }

            return view;
        }

        Transform GetLayerParent(UILayer layer)
        {
            Canvas canvas = GetRootCanvas();
            if (canvas == null) return transform;

            Transform layerParent = UILayerManager.GetLayer(canvas, layer);
            return layerParent != null ? layerParent : canvas.transform;
        }

        Canvas GetRootCanvas()
        {
            if (_rootCanvas != null)
                return _rootCanvas;

            _rootCanvas = GetComponentInParent<Canvas>();
            if (_rootCanvas != null)
                return _rootCanvas;

            _rootCanvas = Object.FindAnyObjectByType<Canvas>();
            return _rootCanvas;
        }

        void HideBattleRewardPopup()
        {
            if (_battleRewardPopup != null)
                _battleRewardPopup.Hide();
        }

        public void ShowBattleResult(bool playerWon)
        {
            EnsureResultPanel();
            if (_resultPanel == null) return;

            _resultTitleText.text = playerWon ? "挑战胜利" : "挑战失败";
            _resultMessageText.text = playerWon ? "确认后重新开始。" : "出牌轮数已耗尽，确认后重新开始。";
            _resultButtonImage.color = playerWon
                ? new Color(0.24f, 0.56f, 0.32f, 1f)
                : new Color(0.24f, 0.45f, 0.85f, 1f);

            ColorBlock colors = _resultConfirmButton.colors;
            colors.highlightedColor = Color.Lerp(_resultButtonImage.color, Color.white, 0.22f);
            colors.pressedColor = Color.Lerp(_resultButtonImage.color, Color.black, 0.22f);
            _resultConfirmButton.colors = colors;

            _resultPanel.transform.SetAsLastSibling();
            _resultPanel.SetActive(true);
        }

        void EnsureResultPanel()
        {
            if (_resultPanel != null) return;

            Transform parent = GetLayerParent(UILayer.System);
            _resultPanel = CreateResultPanel(parent);
            _resultPanel.SetActive(false);
        }

        GameObject CreateResultPanel(Transform parent)
        {
            var panel = new GameObject("BattleResultPopup", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            var rootRect = (RectTransform)panel.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image overlay = panel.GetComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.62f);

            GameObject dialog = new GameObject("Dialog", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            dialog.transform.SetParent(panel.transform, false);

            var dialogRect = (RectTransform)dialog.transform;
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.anchoredPosition = Vector2.zero;
            dialogRect.sizeDelta = new Vector2(420f, 220f);

            Image dialogBackground = dialog.GetComponent<Image>();
            dialogBackground.color = new Color(0.08f, 0.09f, 0.11f, 0.98f);

            VerticalLayoutGroup layout = dialog.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 22, 24);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            _resultTitleText = CreateResultPanelText(dialog.transform, 30, FontStyles.Bold, 44f);
            _resultMessageText = CreateResultPanelText(dialog.transform, 20, FontStyles.Normal, 48f);
            _resultConfirmButton = CreateResultConfirmButton(dialog.transform, out _resultButtonImage);
            _resultConfirmButton.onClick.AddListener(OnResultConfirmClicked);

            return panel;
        }

        TMPro.TextMeshProUGUI CreateResultPanelText(Transform parent, int fontSize, FontStyles fontStyle, float height)
        {
            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rectTransform = (RectTransform)textObject.transform;
            rectTransform.sizeDelta = new Vector2(360f, height);

            var textComponent = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.alignment = TMPro.TextAlignmentOptions.Center;
            textComponent.color = Color.white;
            textComponent.raycastTarget = false;
            return textComponent;
        }

        Button CreateResultConfirmButton(Transform parent, out Image buttonImage)
        {
            var buttonObject = new GameObject("ConfirmButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rectTransform = (RectTransform)buttonObject.transform;
            rectTransform.sizeDelta = new Vector2(180f, 46f);

            buttonImage = buttonObject.GetComponent<Image>();

            Button button = buttonObject.GetComponent<Button>();

            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textComponent = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            textComponent.text = "确认";
            textComponent.fontSize = 22;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.alignment = TMPro.TextAlignmentOptions.Center;
            textComponent.color = Color.white;
            textComponent.raycastTarget = false;

            return button;
        }

        void OnResultConfirmClicked()
        {
            if (_resultPanel != null)
                _resultPanel.SetActive(false);

            this.SendCommand<RestartBattleCommand>();
        }

        void ReleasePopup(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.IsValid())
                Addressables.ReleaseInstance(handle);
        }
    }
}
