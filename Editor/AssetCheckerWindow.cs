using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecking.Editor
{
    public partial class AssetCheckerWindow : EditorWindow, IHasCustomMenu
    {
        #region Static

        internal const string LogTag = "AssetChecker";


        [MenuItem("Tools/Bamboo/Asset Checker")]
        private static void Open()
        {
            GetWindow<AssetCheckerWindow>("AssetCheckerWindow");
        }

        public static AssetCheckerWindow Open(AssetCheckerSettings settings = null)
        {
            AssetCheckerWindow window = GetWindow<AssetCheckerWindow>("AssetCheckerWindow");
            if (settings)
            {
                window.SetSettingsAsset(settings);
            }

            return window;
        }

        #endregion


        [SerializeField]
        private AssetCheckerSettings _settings;
        private CheckResultStats _stats;
        private readonly List<AssetCheckResult> _checkResults = new List<AssetCheckResult>();
        private readonly List<string> _resultCategories = new List<string> { AssetCheckerLocalCache.AllCategories };
        private readonly List<AssetCheckResult> _filteredCheckResults = new List<AssetCheckResult>();
        internal AssetCheckerLocalCache LocalCache => AssetCheckerLocalCache.instance;


        #region Unity Message

        private void OnEnable()
        {
            _settings = LocalCache.GetSettingsAsset();
            _stats = LocalCache.GetCheckResultStats();
            _checkResults.AddRange(LocalCache.GetCheckResults());
            UpdateFilteredCheckResults();
        }

        private void OnFocus()
        {
            LocalCache.InstantCustomViewProvider = _settings?.customViewProvider;

            // NOTE: If the editor window is already open, when there's a change in the script code (triggering compilation), 
            // Unity may call OnFocus before calling OnEnable, so it's necessary to check if the UI controls are null inside the method. 
            // Be careful not to cache a bool value to indicate whether the UI has been created, 
            // because in this situation, the UI controls will be set to null while the bool value remains unchanged.
            UpdateExecutionControls();
        }

        #endregion


        public void Execute()
        {
            if (!_settings)
            {
                string errorMessage = "Execute asset checker failed. Settings not specified.";
                Debug.LogError($"[{LogTag}] {errorMessage}", this);
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                return;
            }

            if (!_settings.assetProvider)
            {
                string errorMessage = "Execute asset checker failed. Asset provider not specified in the settings.";
                Debug.LogError($"[{LogTag}] {errorMessage}", _settings);
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                return;
            }

            AssetChecker[] checkers = _settings.assetCheckers;
            if (checkers == null || checkers.Length == 0)
            {
                string errorMessage = "Execute asset checker failed. Asset checker not specified in the settings.";
                Debug.LogError($"[{LogTag}] {errorMessage}", _settings);
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                return;
            }

            _checkResults.Clear();
            IReadOnlyList<UObject> assets = _settings.assetProvider.GetAssets();
            if (assets == null || assets.Count == 0)
            {
                UpdatePersistentResultData();
                UpdateFilteredCheckResults();
                UpdateResultControls(true);
                return;
            }

            bool hasNullChecker = false;
            for (int i = 0; i < assets.Count; i++)
            {
                UObject asset = assets[i];
                for (int j = 0; j < checkers.Length; j++)
                {
                    AssetChecker checker = checkers[j];
                    if (!checker)
                    {
                        hasNullChecker = true;
                        continue;
                    }

                    try
                    {
                        AssetCheckResult result = checker.CheckAsset(asset);
                        if (result != null)
                        {
                            _checkResults.Add(result);
                        }
                    }
                    catch (Exception e)
                    {
                        AssetCheckResult result = new AssetCheckResult
                        {
                            type = CheckResultType.Exception,
                            categories = new string[] { "Exception" },
                            title = e.GetType().Name,
                            details = e.Message,
                            asset = asset,
                            checker = checker,
                            repairable = false,
                            customData = null,
                            customViewId = null,
                        };
                        _checkResults.Add(result);
                    }
                }
            }

            UpdatePersistentResultData();
            UpdateFilteredCheckResults();
            UpdateResultControls(true);

            if (hasNullChecker)
            {
                string errorMessage = "There are null asset checkers in the settings, please check.";
                Debug.LogError($"[{LogTag}] {errorMessage}", _settings);
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
            }
        }

        public void SetCheckResultTypeFilter(CheckResultTypes filter)
        {
            _resultTypeFilterField.value = filter;
        }

        public void SetCheckResultCategoryFilter(string category)
        {
            category = string.IsNullOrWhiteSpace(category)
                ? AssetCheckerLocalCache.AllCategories
                : category.Trim();
            _resultCategoryFilterField.value = category;
        }

        private void OnResultTypeFilterChanged(ChangeEvent<Enum> evt)
        {
            LocalCache.SetCheckResultTypeFilter((CheckResultTypes)evt.newValue);
            UpdateFilteredCheckResults();
            UpdateResultControls(true);
        }

        private void OnResultCategoryFilterChanged(ChangeEvent<string> evt)
        {
            LocalCache.SetCheckResultCategoryFilter(evt.newValue);
            UpdateFilteredCheckResults();
            UpdateResultControls(true);
        }

        public AssetCheckResult[] GetCheckResults()
        {
            return _checkResults.ToArray();
        }

        public void ClearCheckResults()
        {
            _checkResults.Clear();
            UpdatePersistentResultData();
            UpdateFilteredCheckResults();
            UpdateResultControls(true);
        }

        public AssetCheckerSettings GetSettingsAsset()
        {
            return _settings;
        }

        public void SetSettingsAsset(AssetCheckerSettings settings)
        {
            _settingsField.value = settings;
        }

        private void OnSettingsObjectChanged(ChangeEvent<UObject> evt)
        {
            LocalCache.SetSettingsAsset(_settings);
            UpdateExecutionControls();
        }

        public AssetCheckerSettings CreateSettingsAsset()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Create new settings asset",
                nameof(AssetCheckerSettings), "asset", null);
            if (string.IsNullOrEmpty(savePath))
            {
                return null;
            }

            AssetCheckerSettings settings = CreateInstance<AssetCheckerSettings>();
            AssetDatabase.CreateAsset(settings, savePath);
            EditorGUIUtility.PingObject(settings);

            return settings;
        }

        public ResultIconStyle GetCheckResultIconStyle()
        {
            return LocalCache.GetCheckResultIconStyle();
        }

        public void SetCheckResultIconStyle(ResultIconStyle iconStyle)
        {
            LocalCache.SetCheckResultIconStyle(iconStyle);
            _resultListView.Rebuild();
        }

        private void OnAssetRechecked(AssetCheckResult newResult, AssetCheckResult oldResult)
        {
            bool clearSelection = false;
            if (newResult == null)
            {
                _checkResults.Remove(oldResult);
                clearSelection = true;
            }
            else
            {
                int resultIndex = _checkResults.IndexOf(oldResult);
                _checkResults[resultIndex] = newResult;
            }

            UpdatePersistentResultData();
            UpdateFilteredCheckResults();
            UpdateResultControls(clearSelection);
        }

        private void OnAssetRepaired(AssetCheckResult newResult, bool allIssuesRepaired)
        {
            if (allIssuesRepaired)
            {
                _checkResults.Remove(newResult);
            }

            UpdatePersistentResultData();
            UpdateFilteredCheckResults();
            UpdateResultControls(allIssuesRepaired);
        }

        private void UpdatePersistentResultData()
        {
            _stats.Reset();
            _resultCategories.Clear();

            for (int i = 0; i < _checkResults.Count; i++)
            {
                AssetCheckResult result = _checkResults[i];

                // Stats
                switch (result.type)
                {
                    case CheckResultType.AllPass:
                        _stats.allPass++;
                        break;
                    case CheckResultType.NotImportant:
                        _stats.notImportant++;
                        break;
                    case CheckResultType.Warning:
                        _stats.warning++;
                        break;
                    case CheckResultType.Error:
                        _stats.error++;
                        break;
                    case CheckResultType.Exception:
                        _stats.exception++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result.type), result.type, null);
                }
            }

            LocalCache.SetCheckResultStats(_stats);
            LocalCache.SetCheckResults(_checkResults);
        }

        private void UpdateFilteredCheckResults()
        {
            CheckResultTypes selectedTypes = LocalCache.GetCheckResultTypeFilter();
            string selectedCategory = LocalCache.GetCheckResultCategoryFilter();

            _filteredCheckResults.Clear();
            HashSet<string> categories = new HashSet<string>();

            for (int i = 0; i < _checkResults.Count; i++)
            {
                AssetCheckResult result = _checkResults[i];
                if (result.categories != null)
                {
                    foreach (string category in result.categories)
                    {
                        if (string.IsNullOrWhiteSpace(category))
                        {
                            continue;
                        }

                        string trimmedCategory = category.Trim();
                        if (trimmedCategory == AssetCheckerLocalCache.AllCategories)
                        {
                            continue;
                        }

                        categories.Add(trimmedCategory);
                    }
                }

                CheckResultTypes resultType = (CheckResultTypes)result.type;
                if ((resultType & selectedTypes) == 0)
                {
                    continue;
                }

                if (selectedCategory == AssetCheckerLocalCache.AllCategories ||
                   (result.categories?.Contains(selectedCategory) ?? false))
                {
                    _filteredCheckResults.Add(result);
                }
            }

            _resultCategories.AddRange(categories);
            _resultCategories.RemoveAll(category => string.IsNullOrWhiteSpace(category) || category == AssetCheckerLocalCache.AllCategories);
            _resultCategories.Insert(0, AssetCheckerLocalCache.AllCategories); // Option for no category filter
        }


        #region Custom Menu

        public void AddItemsToMenu(GenericMenu menu)
        {
            // Clear Check Results
            menu.AddItem(new GUIContent("Clear Check Results"), false, ClearCheckResults);
            menu.AddSeparator("");

            // Result Icon Style
            menu.AddItem(new GUIContent("Result Icon Style/Style 1"),
                LocalCache.GetCheckResultIconStyle() == ResultIconStyle.Style1,
                () =>
                {
                    LocalCache.SetCheckResultIconStyle(ResultIconStyle.Style1);
                    _resultListView.Rebuild();
                });
            menu.AddItem(new GUIContent("Result Icon Style/Style 2"),
                LocalCache.GetCheckResultIconStyle() == ResultIconStyle.Style2,
                () =>
                {
                    LocalCache.SetCheckResultIconStyle(ResultIconStyle.Style2);
                    _resultListView.Rebuild();
                });
            menu.AddItem(new GUIContent("Result Icon Style/Style 3"),
                LocalCache.GetCheckResultIconStyle() == ResultIconStyle.Style3,
                () =>
                {
                    LocalCache.SetCheckResultIconStyle(ResultIconStyle.Style3);
                    _resultListView.Rebuild();
                });
            menu.AddSeparator("");

            // Debug
            menu.AddItem(new GUIContent("[Debug] Inspect Local Cache Asset"), false, () =>
            {
                Selection.activeObject = LocalCache;
            });
        }

        #endregion
    }
}