using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GBG.AssetChecking.Editor
{
    [FilePath("Library/com.greenbamboogames.gametoolkit/AssetChecker",
        FilePathAttribute.Location.ProjectFolder)]
    internal class AssetCheckerLocalCache : ScriptableSingleton<AssetCheckerLocalCache>
    {
        public const string AllCategories = "All Categories";

        [SerializeField]
        private AssetCheckerSettings _settingsAsset;
        [SerializeField]
        private CheckResultStats _checkResultStats = new CheckResultStats();
        [SerializeField]
        private AssetCheckResult[] _checkResults = Array.Empty<AssetCheckResult>();
        [SerializeField]
        private CheckResultTypes _resultTypeFilter =
#if UNITY_2022_3_OR_NEWER
            CheckResultTypes.AllTypes; 
#else
            (CheckResultTypes)~0U;
#endif
        [SerializeField]
        private string _resultCategoryFilter = AllCategories;
        [SerializeField]
        private ResultIconStyle _resultIconStyle = ResultIconStyle.Style2;

        public CustomViewProvider InstantCustomViewProvider { get; set; }


        public AssetCheckerSettings GetSettingsAsset()
        {
            return _settingsAsset;
        }

        public void SetSettingsAsset(AssetCheckerSettings settings)
        {
            _settingsAsset = settings;
            Save(true);
        }

        public AssetCheckResult[] GetCheckResults()
        {
            return _checkResults.ToArray();
        }

        public void SetCheckResults(IEnumerable<AssetCheckResult> checkResults)
        {
            _checkResults = checkResults?.ToArray() ?? Array.Empty<AssetCheckResult>();
            Save(true);
        }

        public CheckResultTypes GetCheckResultTypeFilter()
        {
            return _resultTypeFilter;
        }

        public void SetCheckResultTypeFilter(CheckResultTypes types)
        {
            _resultTypeFilter = types;
            Save(true);
        }

        public string GetCheckResultCategoryFilter()
        {
            return _resultCategoryFilter;
        }

        public void SetCheckResultCategoryFilter(string category)
        {
            _resultCategoryFilter = string.IsNullOrWhiteSpace(category)
                ? AllCategories
                : category.Trim();
            Save(true);
        }

        public CheckResultStats GetCheckResultStats()
        {
            CheckResultStats stats = (CheckResultStats)_checkResultStats.Clone();
            return stats;
        }

        public void SetCheckResultStats(CheckResultStats stats)
        {
            _checkResultStats = stats == null
                ? new CheckResultStats()
                : (CheckResultStats)stats.Clone();
            Save(true);
        }

        public ResultIconStyle GetCheckResultIconStyle()
        {
            return _resultIconStyle;
        }

        public void SetCheckResultIconStyle(ResultIconStyle iconStyle)
        {
            _resultIconStyle = iconStyle;
            Save(true);
        }
    }
}