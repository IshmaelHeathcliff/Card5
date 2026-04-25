using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        static readonly Type ScriptableObjectType = typeof(ScriptableObject);

        GlobalConfigPage _globalConfigPage;
        CreateConfigPage _createConfigPage;
        ConfigOverviewPage _overviewPage;

        [MenuItem("Card5/配置中心")]
        static void Open()
        {
            var window = GetWindow<ConfigCenterWindow>();
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
            _createConfigPage.Created -= ForceMenuTreeRebuild;
            _createConfigPage.Created += ForceMenuTreeRebuild;
            _overviewPage.Refresh();
        }

        protected override void OnDisable()
        {
            if (_globalConfigPage != null)
                _globalConfigPage.Changed -= ForceMenuTreeRebuild;
            if (_createConfigPage != null)
                _createConfigPage.Created -= ForceMenuTreeRebuild;
            base.OnDisable();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            EnsurePages();
            _globalConfigPage.RefreshGlobalConfigReference();
            _overviewPage.Refresh();

            var tree = new OdinMenuTree(true)
            {
                DefaultMenuStyle = OdinMenuStyle.TreeViewStyle
            };

            tree.Config.DrawSearchToolbar = true;
            tree.Add("全局配置", _globalConfigPage);
            tree.Add("新建配置", _createConfigPage);
            tree.Add("配置概览", _overviewPage);

            AddAssetsByDirectory(tree);
            AddAssetsByType(tree);
            AddTableViews(tree);

            return tree;
        }

        void EnsurePages()
        {
            if (_globalConfigPage == null)
                _globalConfigPage = new GlobalConfigPage();
            if (_createConfigPage == null)
                _createConfigPage = new CreateConfigPage();
            if (_overviewPage == null)
                _overviewPage = new ConfigOverviewPage();
        }

        static void AddAssetsByDirectory(OdinMenuTree tree)
        {
            if (!AssetDatabase.IsValidFolder(DefaultDataRoot)) return;
            tree.AddAllAssetsAtPath("全部配置/按目录", DefaultDataRoot, ScriptableObjectType, true, false);
        }

        static void AddAssetsByType(OdinMenuTree tree)
        {
            foreach (ScriptableObject asset in FindAllConfigAssets())
            {
                if (asset == null) continue;

                Type type = asset.GetType();
                string typeName = ObjectNames.NicifyVariableName(type.Name);
                string assetPath = AssetDatabase.GetAssetPath(asset);
                string directory = Path.GetDirectoryName(assetPath)?.Replace("\\", "/") ?? DefaultDataRoot;
                string displayPath = directory.StartsWith(DefaultDataRoot)
                    ? directory.Substring(DefaultDataRoot.Length).Trim('/')
                    : directory;

                if (string.IsNullOrEmpty(displayPath))
                    displayPath = "根目录";

                tree.Add($"全部配置/按类型/{typeName}/{displayPath}/{asset.name}", asset);
            }
        }

        static void AddTableViews(OdinMenuTree tree)
        {
            foreach (var group in FindAllConfigAssets().GroupBy(asset => asset.GetType()))
            {
                Type type = group.Key;
                string typeName = ObjectNames.NicifyVariableName(type.Name);
                tree.Add($"表格视图/{typeName}", new ConfigTablePage(type, group.ToList()));
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
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            return assets
                .OrderBy(asset => asset.GetType().Name)
                .ThenBy(asset => AssetDatabase.GetAssetPath(asset))
                .ToList();
        }

        static void EnsurePresetRoot()
        {
            if (AssetDatabase.IsValidFolder(DefaultPresetRoot)) return;
            if (!AssetDatabase.IsValidFolder(DefaultDataRoot))
                AssetDatabase.CreateFolder("Assets", "Data");
            AssetDatabase.CreateFolder(DefaultDataRoot, "Preset");
        }

        static GameGlobalConfigData LoadOrCreateGlobalConfig()
        {
            EnsurePresetRoot();

            var config = AssetDatabase.LoadAssetAtPath<GameGlobalConfigData>(DefaultGlobalConfigPath);
            if (config != null) return config;

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
            if (gameManager == null) return;

            var gameManagerObject = new SerializedObject(gameManager);
            var configObject = new SerializedObject(config);

            CopyObjectReference(gameManagerObject, configObject, "_startingDeck");
            CopyObjectReference(gameManagerObject, configObject, "_enemyData");
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
            if (source == null || target == null) return;
            target.objectReferenceValue = source.objectReferenceValue;
        }

        static void CopyInt(SerializedObject from, SerializedObject to, string propertyName)
        {
            SerializedProperty source = from.FindProperty(propertyName);
            SerializedProperty target = to.FindProperty(propertyName);
            if (source == null || target == null) return;
            target.intValue = source.intValue;
        }

        class GlobalConfigPage
        {
            public event Action Changed;

            [ShowInInspector, InlineEditor(InlineEditorObjectFieldModes.Boxed), LabelText("全局配置")]
            GameGlobalConfigData _globalConfig;

            public void RefreshGlobalConfigReference()
            {
                if (_globalConfig == null)
                    _globalConfig = AssetDatabase.LoadAssetAtPath<GameGlobalConfigData>(DefaultGlobalConfigPath);
            }

            [Button("加载/创建默认全局配置", ButtonSizes.Large), GUIColor(0.3f, 0.75f, 1f)]
            void EnsureGlobalConfig()
            {
                _globalConfig = LoadOrCreateGlobalConfig();
                Selection.activeObject = _globalConfig;
                Changed?.Invoke();
            }

            [Button("从当前场景 GameManager 同步到全局配置")]
            [EnableIf(nameof(HasGlobalConfig))]
            void PullFromSceneGameManager()
            {
                CopySceneGameManagerValues(_globalConfig);
                AssetDatabase.SaveAssets();
                Changed?.Invoke();
            }

            [Button("把全局配置应用到当前场景 GameManager")]
            [EnableIf(nameof(HasGlobalConfig))]
            void ApplyToSceneGameManager()
            {
                GameManager gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                if (gameManager == null)
                {
                    Debug.LogWarning("[ConfigCenter] 当前场景没有 GameManager。");
                    return;
                }

                var serializedObject = new SerializedObject(gameManager);
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

            [Button("在 Project 中选中全局配置")]
            [EnableIf(nameof(HasGlobalConfig))]
            void SelectGlobalConfig()
            {
                Selection.activeObject = _globalConfig;
                EditorGUIUtility.PingObject(_globalConfig);
            }

            bool HasGlobalConfig => _globalConfig != null;
        }

        class CreateConfigPage
        {
            public event Action Created;

            [Title("创建 ScriptableObject 配置")]
            [SerializeField, FolderPath(ParentFolder = "Assets"), LabelText("目标目录")]
            string _targetFolder = DefaultPresetRoot;

            [SerializeField, LabelText("配置类型"), ValueDropdown(nameof(GetConfigTypeOptions))]
            string _selectedTypeName;

            [SerializeField, LabelText("资产名称")]
            string _assetName = "NewConfig";

            [SerializeField, ToggleLeft, LabelText("显示包和插件里的 ScriptableObject 类型")]
            bool _includePackageTypes;

            [Button("创建配置资产", ButtonSizes.Large), GUIColor(0.45f, 0.9f, 0.45f)]
            void CreateAsset()
            {
                Type type = ResolveSelectedType();
                if (type == null)
                {
                    Debug.LogWarning("[ConfigCenter] 请选择有效配置类型。");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_assetName))
                    _assetName = $"New{type.Name}";

                string folder = NormalizeFolder(_targetFolder);
                EnsureFolder(folder);

                string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{_assetName}.asset");
                ScriptableObject asset = CreateInstance(type);
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
                Created?.Invoke();
            }

            IEnumerable<ValueDropdownItem<string>> GetConfigTypeOptions()
            {
                return TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                    .Where(IsCreatableConfigType)
                    .OrderBy(type => type.Namespace)
                    .ThenBy(type => type.Name)
                    .Select(type =>
                    {
                        string assemblyName = type.Assembly.GetName().Name;
                        string path = $"{assemblyName}/{ObjectNames.NicifyVariableName(type.Name)}";
                        return new ValueDropdownItem<string>(path, type.AssemblyQualifiedName);
                    });
            }

            bool IsCreatableConfigType(Type type)
            {
                if (type == null) return false;
                if (type.IsAbstract || type.IsGenericType) return false;
                if (typeof(UnityEditor.Editor).IsAssignableFrom(type)) return false;
                if (!_includePackageTypes && !IsProjectRuntimeType(type)) return false;
                return true;
            }

            static bool IsProjectRuntimeType(Type type)
            {
                string assemblyName = type.Assembly.GetName().Name;
                return assemblyName == "Assembly-CSharp" || assemblyName == "Assembly-CSharp-firstpass";
            }

            Type ResolveSelectedType()
            {
                if (string.IsNullOrEmpty(_selectedTypeName)) return null;
                return Type.GetType(_selectedTypeName);
            }

            static string NormalizeFolder(string folder)
            {
                if (string.IsNullOrWhiteSpace(folder)) return DefaultPresetRoot;
                folder = folder.Replace("\\", "/");
                return folder.StartsWith("Assets") ? folder : $"Assets/{folder.TrimStart('/')}";
            }

            static void EnsureFolder(string folder)
            {
                if (AssetDatabase.IsValidFolder(folder)) return;

                string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
                if (string.IsNullOrEmpty(parent))
                    parent = "Assets";

                if (!AssetDatabase.IsValidFolder(parent))
                    EnsureFolder(parent);

                string name = Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        class ConfigOverviewPage
        {
            [ShowInInspector, ReadOnly, LabelText("配置数量")]
            int _assetCount;

            [ShowInInspector, ReadOnly, LabelText("类型数量")]
            int _typeCount;

            [ShowInInspector, TableList, LabelText("配置类型统计")]
            List<ConfigTypeSummary> _summaries = new List<ConfigTypeSummary>();

            [Button("刷新统计")]
            public void Refresh()
            {
                List<ScriptableObject> assets = FindAllConfigAssets();
                _assetCount = assets.Count;
                _typeCount = assets.Select(asset => asset.GetType()).Distinct().Count();
                _summaries = assets
                    .GroupBy(asset => asset.GetType())
                    .Select(group => new ConfigTypeSummary
                    {
                        TypeName = ObjectNames.NicifyVariableName(group.Key.Name),
                        Count = group.Count(),
                        Folder = GetCommonFolder(group.Select(AssetDatabase.GetAssetPath))
                    })
                    .OrderBy(summary => summary.TypeName)
                    .ToList();
            }

            static string GetCommonFolder(IEnumerable<string> paths)
            {
                string first = paths.FirstOrDefault();
                if (string.IsNullOrEmpty(first)) return string.Empty;
                return Path.GetDirectoryName(first)?.Replace("\\", "/") ?? string.Empty;
            }
        }

        class ConfigTablePage
        {
            readonly Type _configType;

            [ShowInInspector, ReadOnly, LabelText("配置类型")]
            string TypeName => ObjectNames.NicifyVariableName(_configType.Name);

            [ShowInInspector, ReadOnly, LabelText("配置数量")]
            int AssetCount => _rows.Count;

            [ShowInInspector, TableList(AlwaysExpanded = true), LabelText("配置表格")]
            List<ConfigAssetTableRow> _rows = new List<ConfigAssetTableRow>();

            public ConfigTablePage(Type configType, List<ScriptableObject> assets)
            {
                _configType = configType;
                SetAssets(assets);
            }

            [Button("刷新表格")]
            public void Refresh()
            {
                SetAssets(FindAllConfigAssets()
                    .Where(asset => asset.GetType() == _configType)
                    .ToList());
            }

            [Button("在 Project 中多选全部")]
            void SelectAllAssets()
            {
                Selection.objects = _rows
                    .Where(row => row.Asset != null)
                    .Select(row => (UnityEngine.Object)row.Asset)
                    .ToArray();
            }

            [Button("保存全部修改", ButtonSizes.Large), GUIColor(0.45f, 0.9f, 0.45f)]
            void SaveAll()
            {
                foreach (ConfigAssetTableRow row in _rows)
                {
                    if (row.Asset == null) continue;
                    EditorUtility.SetDirty(row.Asset);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Refresh();
            }

            void SetAssets(List<ScriptableObject> assets)
            {
                _rows = assets
                    .OrderBy(asset => AssetDatabase.GetAssetPath(asset))
                    .Select(asset => new ConfigAssetTableRow(asset))
                    .ToList();
            }
        }

        class ConfigAssetTableRow
        {
            readonly ScriptableObject _asset;

            [TableColumnWidth(120, false), ShowInInspector, ReadOnly, LabelText("名称"), PropertyOrder(0)]
            string Name => _asset != null ? _asset.name : string.Empty;

            [TableColumnWidth(240), ShowInInspector, ReadOnly, LabelText("路径"), PropertyOrder(10)]
            string Path => _asset != null ? AssetDatabase.GetAssetPath(_asset) : string.Empty;

            [TableColumnWidth(680), ShowInInspector, InlineEditor(InlineEditorObjectFieldModes.Boxed), LabelText("配置文件 Asset"), PropertyOrder(30)]
            public ScriptableObject Asset => _asset;

            public ConfigAssetTableRow(ScriptableObject asset)
            {
                _asset = asset;
            }

            [VerticalGroup("操作"), TableColumnWidth(96, false), PropertyOrder(20)]
            [Button("选中")]
            void Select()
            {
                if (_asset == null) return;
                Selection.activeObject = _asset;
                EditorGUIUtility.PingObject(_asset);
            }

            [VerticalGroup("操作"), PropertyOrder(21)]
            [Button("保存")]
            void Save()
            {
                if (_asset == null) return;
                EditorUtility.SetDirty(_asset);
                AssetDatabase.SaveAssets();
            }
        }

        class ConfigTypeSummary
        {
            [TableColumnWidth(180)]
            public string TypeName;

            [TableColumnWidth(60)]
            public int Count;

            public string Folder;
        }
    }
}
