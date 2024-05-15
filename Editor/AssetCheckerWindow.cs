using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UDebug = UnityEngine.Debug;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecking.Editor
{
    public partial class AssetCheckerWindow : EditorWindow, IHasCustomMenu
    {
        #region Static

        [MenuItem("Tools/Bamboo/Asset Checker")]
        [MenuItem("Window/Asset Management/Asset Checker")]
        private static void Open()
        {
            Open(null);
        }

        public static AssetCheckerWindow Open(AssetCheckerSettings settings = null)
        {
            AssetCheckerWindow window = GetWindow<AssetCheckerWindow>("Asset Checker");
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
        private int _nullResultCount;
        private readonly List<AssetCheckResult> _checkResults = new List<AssetCheckResult>();
        private readonly List<string> _resultCategories = new List<string> { AssetCheckerLocalCache.AllCategories };
        private readonly List<AssetCheckResult> _filteredCheckResults = new List<AssetCheckResult>();
        internal static AssetCheckerLocalCache LocalCache => AssetCheckerLocalCache.instance;


        #region Unity Message

        private void OnEnable()
        {
            _settings = LocalCache.GetSettingsAsset();
            _stats = LocalCache.GetCheckResultStats();
            _nullResultCount = _stats.nullResult;
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
                string errorMessage = "Asset checker execution failed. Settings not specified.";
                UDebug.LogError($"[{AssetChecker.LogTag}] {errorMessage}", this);
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                return;
            }

            if (!_settings.assetProvider)
            {
                string errorMessage = "Asset checker execution failed. Asset provider not specified in the settings.";
                UDebug.LogError($"[{AssetChecker.LogTag}] {errorMessage}", _settings);
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                return;
            }

            AssetChecker[] checkers = _settings.assetCheckers;
            if (checkers == null || checkers.Length == 0)
            {
                string errorMessage = "Asset checker execution failed. Asset checker not specified in the settings.";
                UDebug.LogError($"[{AssetChecker.LogTag}] {errorMessage}", _settings);
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                return;
            }

            ExecutionResult executionResult = AssetChecker.Execute(_settings.assetProvider,
                _settings.assetCheckers, _checkResults, out _nullResultCount, logContext: _settings);

            if (executionResult == ExecutionResult.Failure)
            {
                return;
            }

            UpdatePersistentResultData();
            UpdateFilteredCheckResults();
            UpdateResultControls(true);
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

        private void OnShowResultEntryAssetIconChanged(ChangeEvent<bool> evt)
        {
            LocalCache.SetShowResultEntryAssetIcon(evt.newValue);
            RefreshResultListView();
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
            _settings = (AssetCheckerSettings)evt.newValue; // Binding not work on Unity 2020
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

        public ResultTypeIconStyle GetCheckResultIconStyle()
        {
            return LocalCache.GetCheckResultTypeIconStyle();
        }

        public void SetCheckResultIconStyle(ResultTypeIconStyle iconStyle)
        {
            LocalCache.SetCheckResultIconStyle(iconStyle);
            RefreshResultListView();
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
            _stats.nullResult = _nullResultCount;

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
                        if (trimmedCategory == AssetCheckerLocalCache.AllCategories ||
                            trimmedCategory == AssetCheckerLocalCache.Repairable)
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

                if ((selectedCategory == AssetCheckerLocalCache.AllCategories) ||
                    (selectedCategory == AssetCheckerLocalCache.Repairable && result.repairable) ||
                    (result.categories?.Contains(selectedCategory) ?? false))
                {
                    _filteredCheckResults.Add(result);
                }
            }

            _resultCategories.Clear();
            _resultCategories.Add(AssetCheckerLocalCache.AllCategories); // Option for no category filter
            _resultCategories.Add(AssetCheckerLocalCache.Repairable);
            _resultCategories.AddRange(categories);
        }


        #region Custom Menu

        public void AddItemsToMenu(GenericMenu menu)
        {
            // Source Code
            menu.AddItem(new GUIContent("Source Code"), false, () =>
            {
                Application.OpenURL("https://github.com/SolarianZ/UnityAssetChecker");
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
