using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Card5.Editor
{
    public class ConfigCenterWindow : OdinMenuEditorWindow
    {
        const string DefaultDataRoot = "Assets/Data";
        const string DefaultPresetRoot = "Assets/Data/Preset";
        const string DefaultGlobalConfigPath = "Assets/Data/Preset/GameGlobalConfig.asset";

        static readonly Dictionary<Type, string> TypeAliases = new Dictionary<Type, string>
        {
            [typeof(CardData)] = "卡牌配置",
            [typeof(DeckPresetData)] = "牌组预设",
            [typeof(EnemyData)] = "敌人配置",
            [typeof(MonsterListData)] = "怪物列表",
            [typeof(MarkData)] = "印记配置",
            [typeof(CardLibraryData)] = "卡牌牌库",
            [typeof(BattleRewardConfigData)] = "战斗奖励配置",
            [typeof(GameGlobalConfigData)] = "全局游戏配置"
        };

        GlobalConfigPage _globalConfigPage;
        ConfigOverviewPage _overviewPage;

        [MenuItem("Card5/配置中心")]
        static void Open()
        {
            ConfigCenterWindow window = GetWindow<ConfigCenterWindow>();
            window.titleContent = new GUIContent("配置中心");
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsurePages();
            _globalConfigPage.RefreshGlobalConfigReference();
            _globalConfigPage.Changed -= ForceMenuTreeRebuild;
            _globalConfigPage.Changed += ForceMenuTreeRebuild;
            _overviewPage.Refresh();
        }

        protected override void OnDisable()
        {
            if (_globalConfigPage != null)
            {
                _globalConfigPage.Changed -= ForceMenuTreeRebuild;
            }

            base.OnDisable();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            EnsurePages();
            _globalConfigPage.RefreshGlobalConfigReference();
            _overviewPage.Refresh();

            OdinMenuTree tree = new OdinMenuTree(true)
            {
                DefaultMenuStyle = OdinMenuStyle.TreeViewStyle
            };

            tree.Config.DrawSearchToolbar = true;
            tree.Add("全局配置", _globalConfigPage);
            tree.Add("配置概览", _overviewPage);

            AddAssetsByType(tree);
            return tree;
        }

        void EnsurePages()
        {
            if (_globalConfigPage == null)
            {
                _globalConfigPage = new GlobalConfigPage();
            }

            if (_overviewPage == null)
            {
                _overviewPage = new ConfigOverviewPage();
            }
        }

        void AddAssetsByType(OdinMenuTree tree)
        {
            List<ScriptableObject> allAssets = FindAllConfigAssets();
            Dictionary<Type, List<ScriptableObject>> assetsByType = allAssets
                .GroupBy(asset => asset.GetType())
                .ToDictionary(group => group.Key, group => group.OrderBy(asset => asset.name).ToList());

            foreach (Type type in GetProjectConfigTypes())
            {
                string typeName = GetTypeDisplayName(type);
                List<ScriptableObject> typeAssets = assetsByType.TryGetValue(type, out List<ScriptableObject> existingAssets)
                    ? existingAssets
                    : new List<ScriptableObject>();

                string typePath = $"全部配置/{typeName}";
                tree.Add(typePath, new ConfigTypePage(type, typeAssets, ForceMenuTreeRebuild));

                foreach (ScriptableObject asset in typeAssets)
                {
                    if (asset == null)
                    {
                        continue;
                    }

                    tree.Add($"{typePath}/{asset.name}", asset);
                }
            }
        }

        static List<ScriptableObject> FindAllConfigAssets()
        {
            string[] folders = AssetDatabase.IsValidFolder(DefaultDataRoot)
                ? new[] { DefaultDataRoot }
                : new[] { "Assets" };

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", folders);
            var assets = new List<ScriptableObject>(guids.Length);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset != null && IsCreatableConfigType(asset.GetType()))
                {
                    assets.Add(asset);
                }
            }

            return assets
                .OrderBy(asset => GetTypeDisplayName(asset.GetType()))
                .ThenBy(asset => asset.name)
                .ToList();
        }

        static List<Type> GetProjectConfigTypes()
        {
            return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(IsCreatableConfigType)
                .OrderBy(GetTypeDisplayName)
                .ThenBy(type => type.Name)
                .ToList();
        }

        static string GetTypeDisplayName(Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            if (TypeAliases.TryGetValue(type, out string alias) && !string.IsNullOrWhiteSpace(alias))
            {
                return alias;
            }

            return ObjectNames.NicifyVariableName(type.Name);
        }

        static bool IsCreatableConfigType(Type type)
        {
            if (type == null) return false;
            if (type.IsAbstract || type.IsGenericType) return false;
            if (typeof(UnityEditor.Editor).IsAssignableFrom(type)) return false;
            if (!IsProjectRuntimeType(type)) return false;
            return IsCard5ConfigType(type);
        }

        static bool IsProjectRuntimeType(Type type)
        {
            string assemblyName = type.Assembly.GetName().Name;
            return assemblyName == "Assembly-CSharp" || assemblyName == "Assembly-CSharp-firstpass";
        }

        static bool IsCard5ConfigType(Type type)
        {
            if (type == null) return false;

            CreateAssetMenuAttribute createAssetMenu = type.GetCustomAttribute<CreateAssetMenuAttribute>();
            return createAssetMenu != null
                && !string.IsNullOrWhiteSpace(createAssetMenu.menuName)
                && createAssetMenu.menuName.StartsWith("Card5/", StringComparison.Ordinal);
        }

        static void EnsurePresetRoot()
        {
            if (AssetDatabase.IsValidFolder(DefaultPresetRoot))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(DefaultDataRoot))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }

            AssetDatabase.CreateFolder(DefaultDataRoot, "Preset");
        }

        static string NormalizeFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return DefaultPresetRoot;
            }

            string normalized = folder.Replace("\\", "/");
            return normalized.StartsWith("Assets", StringComparison.Ordinal)
                ? normalized
                : $"Assets/{normalized.TrimStart('/')}";
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(parent))
            {
                parent = "Assets";
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            string name = Path.GetFileName(folder);
            AssetDatabase.CreateFolder(parent, name);
        }

        static ScriptableObject CreateConfigAsset(Type type, string targetFolder, string assetName)
        {
            string folder = NormalizeFolder(targetFolder);
            EnsureFolder(folder);

            string finalAssetName = string.IsNullOrWhiteSpace(assetName)
                ? $"New{type.Name}"
                : assetName.Trim();

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{finalAssetName}.asset");
            ScriptableObject asset = CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        static GameGlobalConfigData LoadOrCreateGlobalConfig()
        {
            EnsurePresetRoot();

            GameGlobalConfigData config = AssetDatabase.LoadAssetAtPath<GameGlobalConfigData>(DefaultGlobalConfigPath);
            if (config != null)
            {
                return config;
            }

            config = CreateInstance<GameGlobalConfigData>();
            AssetDatabase.CreateAsset(config, DefaultGlobalConfigPath);
            CopySceneGameManagerValues(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return config;
        }

        static void CopySceneGameManagerValues(GameGlobalConfigData config)
        {
            GameManager gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (gameManager == null)
            {
                return;
            }

            SerializedObject gameManagerObject = new SerializedObject(gameManager);
            SerializedObject configObject = new SerializedObject(config);

            CopyObjectReference(gameManagerObject, configObject, "_startingDeck");
            CopyObjectReference(gameManagerObject, configObject, "_rewardConfig");
            CopyInt(gameManagerObject, configObject, "_maxEnergy");
            CopyInt(gameManagerObject, configObject, "_targetFrameRate");

            configObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        static void CopyObjectReference(SerializedObject from, SerializedObject to, string propertyName)
        {
            SerializedProperty source = from.FindProperty(propertyName);
            SerializedProperty target = to.FindProperty(propertyName);
            if (source == null || target == null)
            {
                return;
            }

            target.objectReferenceValue = source.objectReferenceValue;
        }

        static void CopyInt(SerializedObject from, SerializedObject to, string propertyName)
        {
            SerializedProperty source = from.FindProperty(propertyName);
            SerializedProperty target = to.FindProperty(propertyName);
            if (source == null || target == null)
            {
                return;
            }

            target.intValue = source.intValue;
        }

        [Serializable]
        class GlobalConfigPage
        {
            public event Action Changed;

            [ShowInInspector, PropertyOrder(0), TitleGroup("全局配置"), HideLabel]
            [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
            GameGlobalConfigData _globalConfig;

            public void RefreshGlobalConfigReference()
            {
                if (_globalConfig == null)
                {
                    _globalConfig = AssetDatabase.LoadAssetAtPath<GameGlobalConfigData>(DefaultGlobalConfigPath);
                }
            }

            [PropertyOrder(10)]
            [Button("加载或创建默认全局配置", ButtonSizes.Large), GUIColor(0.3f, 0.75f, 1f)]
            void EnsureGlobalConfig()
            {
                _globalConfig = LoadOrCreateGlobalConfig();
                Selection.activeObject = _globalConfig;
                Changed?.Invoke();
            }

            [PropertyOrder(20)]
            [EnableIf(nameof(HasGlobalConfig))]
            [ButtonGroup("全局配置/操作"), Button("从当前场景同步")]
            void PullFromSceneGameManager()
            {
                CopySceneGameManagerValues(_globalConfig);
                AssetDatabase.SaveAssets();
                Changed?.Invoke();
            }

            [PropertyOrder(20)]
            [EnableIf(nameof(HasGlobalConfig))]
            [ButtonGroup("全局配置/操作"), Button("应用到当前场景")]
            void ApplyToSceneGameManager()
            {
                GameManager gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                if (gameManager == null)
                {
                    Debug.LogWarning("[ConfigCenter] 当前场景没有 GameManager。");
                    return;
                }

                SerializedObject serializedObject = new SerializedObject(gameManager);
                SerializedProperty property = serializedObject.FindProperty("_globalConfig");
                if (property == null)
                {
                    Debug.LogWarning("[ConfigCenter] GameManager 没有 _globalConfig 字段。");
                    return;
                }

                property.objectReferenceValue = _globalConfig;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gameManager);
                EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);
                EditorSceneManager.SaveOpenScenes();
                Changed?.Invoke();
            }

            [PropertyOrder(20)]
            [EnableIf(nameof(HasGlobalConfig))]
            [ButtonGroup("全局配置/操作"), Button("在 Project 中选中")]
            void SelectGlobalConfig()
            {
                Selection.activeObject = _globalConfig;
                EditorGUIUtility.PingObject(_globalConfig);
            }

            bool HasGlobalConfig => _globalConfig != null;
        }

        [Serializable]
        class ConfigTypePage
        {
            readonly Type _configType;
            readonly List<ScriptableObject> _assets;
            readonly Action _createdCallback;

            [ShowInInspector, PropertyOrder(0), BoxGroup("概览"), HideLabel]
            ConfigTypeStats _stats;

            [ShowInInspector, PropertyOrder(10), BoxGroup("新建配置"), HideLabel, InlineProperty]
            ConfigCreatePanel _createPanel;

            [ShowInInspector, PropertyOrder(20), BoxGroup("现有配置"), HideLabel]
            [TableList(AlwaysExpanded = true, DrawScrollView = true, IsReadOnly = true)]
            [Searchable]
            List<ConfigAssetSummary> _assetSummaries;

            public ConfigTypePage(Type configType, List<ScriptableObject> assets, Action createdCallback)
            {
                _configType = configType;
                _assets = assets ?? new List<ScriptableObject>();
                _createdCallback = createdCallback;

                _stats = new ConfigTypeStats
                {
                    TypeName = GetTypeDisplayName(_configType),
                    AssetCount = _assets.Count
                };

                _createPanel = new ConfigCreatePanel(_configType, GetSuggestedFolder(), HandleAssetCreated);
                _assetSummaries = _assets.Select(asset => new ConfigAssetSummary(asset)).ToList();
            }

            void HandleAssetCreated()
            {
                _createdCallback?.Invoke();
            }

            string GetSuggestedFolder()
            {
                string firstAssetPath = _assets
                    .Select(AssetDatabase.GetAssetPath)
                    .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path));

                if (string.IsNullOrWhiteSpace(firstAssetPath))
                {
                    return DefaultPresetRoot;
                }

                return Path.GetDirectoryName(firstAssetPath)?.Replace("\\", "/") ?? DefaultPresetRoot;
            }
        }

        [Serializable]
        class ConfigOverviewPage
        {
            [ShowInInspector, PropertyOrder(0), BoxGroup("配置概览"), HideLabel]
            ConfigOverviewStats _stats = new ConfigOverviewStats();

            [ShowInInspector, PropertyOrder(10), BoxGroup("类型统计"), HideLabel]
            [TableList(AlwaysExpanded = true, DrawScrollView = true, IsReadOnly = true)]
            [Searchable]
            List<ConfigTypeSummary> _summaries = new List<ConfigTypeSummary>();

            [PropertyOrder(20)]
            [Button("刷新统计")]
            public void Refresh()
            {
                List<ScriptableObject> assets = FindAllConfigAssets();
                _stats.AssetCount = assets.Count;
                _stats.TypeCount = assets.Select(asset => asset.GetType()).Distinct().Count();
                _summaries = assets
                    .GroupBy(asset => asset.GetType())
                    .Select(group => new ConfigTypeSummary
                    {
                        TypeName = GetTypeDisplayName(group.Key),
                        Count = group.Count()
                    })
                    .OrderBy(summary => summary.TypeName)
                    .ToList();
            }
        }

        [Serializable]
        class ConfigCreatePanel
        {
            readonly Type _configType;
            readonly Action _createdCallback;

            [LabelText("配置类型"), ReadOnly]
            public string TypeName => GetTypeDisplayName(_configType);

            [LabelText("资源名称")]
            public string AssetName;

            [LabelText("目标目录")]
            [FolderPath(ParentFolder = "Assets", AbsolutePath = false)]
            public string TargetFolder;

            public ConfigCreatePanel(Type configType, string targetFolder, Action createdCallback)
            {
                _configType = configType;
                _createdCallback = createdCallback;
                AssetName = $"New{configType.Name}";
                TargetFolder = targetFolder;
            }

            [Button("新建当前类型配置", ButtonSizes.Large), GUIColor(0.45f, 0.9f, 0.45f)]
            void CreateAsset()
            {
                CreateConfigAsset(_configType, TargetFolder, AssetName);
                _createdCallback?.Invoke();
            }
        }

        [Serializable]
        class ConfigTypeStats
        {
            [HorizontalGroup("行"), LabelWidth(60), ReadOnly, LabelText("配置类型")]
            public string TypeName;

            [HorizontalGroup("行"), LabelWidth(60), ReadOnly, LabelText("数量")]
            public int AssetCount;
        }

        [Serializable]
        class ConfigOverviewStats
        {
            [HorizontalGroup("行"), LabelWidth(70), ReadOnly, LabelText("配置数量")]
            public int AssetCount;

            [HorizontalGroup("行"), LabelWidth(70), ReadOnly, LabelText("类型数量")]
            public int TypeCount;
        }

        [Serializable]
        class ConfigAssetSummary
        {
            readonly ScriptableObject _asset;

            [ShowInInspector, ReadOnly, TableColumnWidth(220), LabelText("资源")]
            ScriptableObject Asset => _asset;

            [ShowInInspector, ReadOnly, TableColumnWidth(160), LabelText("名称")]
            string AssetName => _asset != null ? _asset.name : string.Empty;

            public ConfigAssetSummary(ScriptableObject asset)
            {
                _asset = asset;
            }

            [Button("选中"), TableColumnWidth(60)]
            void Select()
            {
                if (_asset == null)
                {
                    return;
                }

                Selection.activeObject = _asset;
                EditorGUIUtility.PingObject(_asset);
            }
        }

        [Serializable]
        class ConfigTypeSummary
        {
            [TableColumnWidth(220), LabelText("配置类型")]
            public string TypeName;

            [TableColumnWidth(70), LabelText("数量")]
            public int Count;
        }
    }
}
