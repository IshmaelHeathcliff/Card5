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
        const string RewardConfigPath = "Assets/Data/Preset/Reward/BattleRewardConfig.asset";
        const string CardLibraryPath = "Assets/Data/Preset/CardLibrary/DefaultCardLibrary.asset";
        const string OptionPrefabPath = "Assets/Prefabs/BattleRewardOption.prefab";
        const string PopupPrefabPath = "Assets/Prefabs/BattleRewardPopup.prefab";

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

        static BattleRewardConfigData CreateRewardConfigAsset()
        {
            EnsureFolder("Assets/Data/Preset/Reward");

            var config = AssetDatabase.LoadAssetAtPath<BattleRewardConfigData>(RewardConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BattleRewardConfigData>();
                AssetDatabase.CreateAsset(config, RewardConfigPath);
            }

            CardLibraryData cardLibrary = CreateCardLibraryAsset();

            var serializedObject = new SerializedObject(config);
            SerializedProperty groups = serializedObject.FindProperty("_rewardGroups");
            groups.arraySize = 1;

            SerializedProperty group = groups.GetArrayElementAtIndex(0);
            group.FindPropertyRelative("_rewardType").enumValueIndex = (int)BattleRewardType.Card;
            group.FindPropertyRelative("_choiceCount").intValue = 3;
            group.FindPropertyRelative("_cardLibrary").objectReferenceValue = cardLibrary;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }

        static CardLibraryData CreateCardLibraryAsset()
        {
            EnsureFolder("Assets/Data/Preset/CardLibrary");

            var cardLibrary = AssetDatabase.LoadAssetAtPath<CardLibraryData>(CardLibraryPath);
            if (cardLibrary == null)
            {
                cardLibrary = ScriptableObject.CreateInstance<CardLibraryData>();
                AssetDatabase.CreateAsset(cardLibrary, CardLibraryPath);
            }

            CardData basic = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Preset/Cards/BasicDamageCard.asset");
            CardData advanced = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Preset/Cards/AdvancedDamageCard.asset");
            CardData great = AssetDatabase.LoadAssetAtPath<CardData>("Assets/Data/Preset/Cards/GreatDamageCard.asset");

            var serializedObject = new SerializedObject(cardLibrary);
            SerializedProperty entries = serializedObject.FindProperty("_entries");
            entries.arraySize = 3;
            SetCardLibraryEntry(entries.GetArrayElementAtIndex(0), basic, 3, 0, 0);
            SetCardLibraryEntry(entries.GetArrayElementAtIndex(1), advanced, 2, 1, (int)CardUnlockConditionType.MinTurnNumber);
            SetCardLibraryEntry(entries.GetArrayElementAtIndex(2), great, 1, 2, (int)CardUnlockConditionType.MinTurnNumber);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cardLibrary);
            return cardLibrary;
        }

        static void SetCardLibraryEntry(SerializedProperty entry, CardData card, int weight, int conditionValue, int conditionType)
        {
            entry.FindPropertyRelative("_card").objectReferenceValue = card;
            entry.FindPropertyRelative("_weight").intValue = weight;

            SerializedProperty conditions = entry.FindPropertyRelative("_unlockConditions");
            bool hasCondition = conditionType != (int)CardUnlockConditionType.Always;
            conditions.arraySize = hasCondition ? 1 : 0;
            if (!hasCondition) return;

            SerializedProperty condition = conditions.GetArrayElementAtIndex(0);
            condition.FindPropertyRelative("_conditionType").enumValueIndex = conditionType;
            condition.FindPropertyRelative("_value").intValue = conditionValue;
            condition.FindPropertyRelative("_card").objectReferenceValue = null;
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;

            string parent = System.IO.Path.GetDirectoryName(folder)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(parent))
                parent = "Assets";

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            string folderName = System.IO.Path.GetFileName(folder);
            AssetDatabase.CreateFolder(parent, folderName);
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
