using System;
using Card5;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Card5.Editor
{
    public static class BattleRewardSetupUtility
    {
        const string RewardConfigPath = "Assets/Data/Preset/BattleRewardConfig.asset";
        const string OptionPrefabPath = "Assets/Prefabs/BattleRewardOption.prefab";
        const string PopupPrefabPath = "Assets/Prefabs/BattleRewardPopup.prefab";
        const string MarkCardPath = "Assets/Data/Preset/Cards/BasicMarkCard.asset";

        [MenuItem("Card5/Setup Battle Reward UI")]
        public static void SetupFromMenu()
        {
            Debug.Log(Run());
        }

        public static string Run()
        {
            BattleRewardConfigData configAsset = CreateRewardConfigAsset();
            GameObject optionPrefab = CreateRewardOptionPrefab();
            GameObject popup = BuildRewardPopupInstance(optionPrefab);
            PrefabUtility.SaveAsPrefabAssetAndConnect(popup, PopupPrefabPath, InteractionMode.AutomatedAction);

            GameManager gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (gameManager == null)
                throw new Exception("Cannot find GameManager component in scene.");

            SetObjectReference(gameManager, "_rewardConfig", configAsset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            return "Created battle reward config and UI.";
        }

        static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
                throw new Exception($"Property not found: {target.GetType().Name}.{propertyName}");

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        static void SetStringValue(UnityEngine.Object target, string propertyName, string value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
                throw new Exception($"Property not found: {target.GetType().Name}.{propertyName}");

            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        static void SetIntValue(UnityEngine.Object target, string propertyName, int value)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
                throw new Exception($"Property not found: {target.GetType().Name}.{propertyName}");

            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        static TextMeshProUGUI CreateTextElement(string name, Transform parent, string text, int size, FontStyles style, TextAlignmentOptions alignment)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);

            var textComponent = gameObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = size;
            textComponent.fontStyle = style;
            textComponent.alignment = alignment;
            textComponent.color = Color.white;
            textComponent.raycastTarget = false;

            var rectTransform = (RectTransform)gameObject.transform;
            rectTransform.sizeDelta = new Vector2(220f, size + 12f);
            return textComponent;
        }

        static Image CreateImageElement(string name, Transform parent, Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);

            var image = gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        static CardData EnsureThirdRewardCard()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CardData>(MarkCardPath);
            if (existing != null) return existing;

            var card = ScriptableObject.CreateInstance<CardData>();
            AssetDatabase.CreateAsset(card, MarkCardPath);

            SetStringValue(card, "_cardId", "BasicMarkCard");
            SetStringValue(card, "_cardName", "印记卡");
            SetStringValue(card, "_description", "施加基础印记。");
            SetIntValue(card, "_energyCost", 1);

            var effect = AssetDatabase.LoadAssetAtPath<CardEffectSO>("Assets/Data/Preset/ApplyMark/ApplyMarkEffect.asset");
            if (effect != null)
            {
                var serializedObject = new SerializedObject(card);
                SerializedProperty effects = serializedObject.FindProperty("_effects");
                effects.arraySize = 1;
                effects.GetArrayElementAtIndex(0).objectReferenceValue = effect;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorUtility.SetDirty(card);
            return card;
        }

        static BattleRewardConfigData CreateRewardConfigAsset()
        {
            var config = AssetDatabase.LoadAssetAtPath<BattleRewardConfigData>(RewardConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BattleRewardConfigData>();
                AssetDatabase.CreateAsset(config, RewardConfigPath);
            }

            CardData damage = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Preset/Cards/BasicDamageCard.asset");
            CardData heal = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Preset/Cards/BasicHealCard.asset");
            CardData mark = EnsureThirdRewardCard();

            var serializedObject = new SerializedObject(config);
            SerializedProperty groups = serializedObject.FindProperty("_rewardGroups");
            groups.arraySize = 1;

            SerializedProperty group = groups.GetArrayElementAtIndex(0);
            group.FindPropertyRelative("_rewardType").enumValueIndex = (int)BattleRewardType.Card;
            group.FindPropertyRelative("_choiceCount").intValue = 3;

            SerializedProperty cardPool = group.FindPropertyRelative("_cardPool");
            cardPool.arraySize = 3;
            cardPool.GetArrayElementAtIndex(0).objectReferenceValue = damage;
            cardPool.GetArrayElementAtIndex(1).objectReferenceValue = heal;
            cardPool.GetArrayElementAtIndex(2).objectReferenceValue = mark;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        static GameObject CreateRewardOptionPrefab()
        {
            var root = new GameObject("BattleRewardOption", typeof(RectTransform));
            var rectTransform = (RectTransform)root.transform;
            rectTransform.sizeDelta = new Vector2(220f, 300f);

            var background = root.AddComponent<Image>();
            background.color = new Color(0.13f, 0.16f, 0.20f, 0.96f);

            var button = root.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.90f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.70f, 0.82f, 1f, 1f);
            button.colors = colors;

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 14, 14);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Image artwork = CreateImageElement("Artwork", root.transform, new Color(0.08f, 0.10f, 0.13f, 1f));
            ((RectTransform)artwork.transform).sizeDelta = new Vector2(190f, 110f);

            TextMeshProUGUI nameText = CreateTextElement("NameText", root.transform, "卡牌", 24, FontStyles.Bold, TextAlignmentOptions.Center);
            TextMeshProUGUI costText = CreateTextElement("CostText", root.transform, "0", 20, FontStyles.Bold, TextAlignmentOptions.Center);
            costText.color = new Color(0.95f, 0.82f, 0.30f, 1f);
            TextMeshProUGUI descriptionText = CreateTextElement("DescriptionText", root.transform, "效果描述", 18, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            ((RectTransform)descriptionText.transform).sizeDelta = new Vector2(190f, 90f);

            var optionView = root.AddComponent<BattleRewardOptionView>();
            SetObjectReference(optionView, "_button", button);
            SetObjectReference(optionView, "_nameText", nameText);
            SetObjectReference(optionView, "_costText", costText);
            SetObjectReference(optionView, "_descriptionText", descriptionText);
            SetObjectReference(optionView, "_artworkImage", artwork);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, OptionPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject BuildRewardPopupInstance(GameObject optionPrefab)
        {
            GameObject canvas = GameObject.Find("View");
            if (canvas == null)
                throw new Exception("Cannot find Canvas GameObject named View.");

            Transform old = canvas.transform.Find("BattleRewardPopup");
            if (old != null)
                UnityEngine.Object.DestroyImmediate(old.gameObject);

            var popup = new GameObject("BattleRewardPopup", typeof(RectTransform));
            popup.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)popup.transform);

            var popupView = popup.AddComponent<BattleRewardPopupView>();

            var root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(popup.transform, false);
            Stretch((RectTransform)root.transform);
            var overlay = root.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.62f);

            var dialog = new GameObject("Dialog", typeof(RectTransform));
            dialog.transform.SetParent(root.transform, false);
            var dialogRect = (RectTransform)dialog.transform;
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRect.anchoredPosition = Vector2.zero;
            dialogRect.sizeDelta = new Vector2(820f, 460f);

            var dialogBackground = dialog.AddComponent<Image>();
            dialogBackground.color = new Color(0.07f, 0.09f, 0.12f, 0.98f);

            var dialogLayout = dialog.AddComponent<VerticalLayoutGroup>();
            dialogLayout.padding = new RectOffset(26, 26, 22, 26);
            dialogLayout.spacing = 18f;
            dialogLayout.childAlignment = TextAnchor.UpperCenter;
            dialogLayout.childControlWidth = true;
            dialogLayout.childControlHeight = false;
            dialogLayout.childForceExpandWidth = true;
            dialogLayout.childForceExpandHeight = false;

            TextMeshProUGUI title = CreateTextElement("TitleText", dialog.transform, "选择奖励", 32, FontStyles.Bold, TextAlignmentOptions.Center);
            ((RectTransform)title.transform).sizeDelta = new Vector2(760f, 48f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(dialog.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.sizeDelta = new Vector2(760f, 320f);

            var contentLayout = content.AddComponent<HorizontalLayoutGroup>();
            contentLayout.spacing = 18f;
            contentLayout.childAlignment = TextAnchor.MiddleCenter;
            contentLayout.childControlWidth = false;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = false;

            SetObjectReference(popupView, "_root", root);
            SetObjectReference(popupView, "_titleText", title);
            SetObjectReference(popupView, "_contentContainer", content.transform);
            SetObjectReference(popupView, "_optionPrefab", optionPrefab.GetComponent<BattleRewardOptionView>());

            root.SetActive(false);
            return popup;
        }
    }
}
