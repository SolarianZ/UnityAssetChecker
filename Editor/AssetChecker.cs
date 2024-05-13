using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UDebug = UnityEngine.Debug;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecking.Editor
{
    public enum ExecutionResult
    {
        Success,
        Failure,
        NoAssets,
    }

    public abstract class AssetChecker : ScriptableObject
    {
        /// <summary>
        /// Execute the asset checking process.
        /// </summary>
        /// <param name="asset">The asset to be checked.</param>
        /// <returns>Check result. Can be null(meaning there are no issues).</returns>
        public abstract AssetCheckResult CheckAsset(UObject asset);

        /// <summary>
        /// Attempt to repair the issues with the assets. 
        /// </summary>
        /// <param name="checkResult">Repair result. Can be null(meaning there are no issues).</param>
        /// <param name="allIssuesRepaired">Whether all issues have been repaired.</param>
        public abstract void RepairAsset(AssetCheckResult checkResult, out bool allIssuesRepaired);


        public const string CategoryName_Exception = "Exception";
        internal protected const string LogTag = "AssetChecker";

        /// <summary>
        /// Launch the asset checking process.
        /// </summary>
        /// <param name="assetProvider">Provides the assets to be checked.</param>
        /// <param name="assetCheckers">The list of checkers that operate on the assets.</param>
        /// <param name="checkResults">The results of the checks.</param>
        /// <param name="nullResultCount">The count of null results returned by checkers (null results are considered as passed checks).</param>
        /// <param name="logContext">The log context (clicking on a log entry in the console will ping this object).</param>
        /// <param name="allowDisplayDialog">Specifies whether to allow displaying popup messages.</param>
        /// <returns>The execution result.</returns>
        public static ExecutionResult Execute(AssetProvider assetProvider,
            IReadOnlyList<AssetChecker> assetCheckers,
            List<AssetCheckResult> checkResults, out int nullResultCount,
            UObject logContext = null, bool allowDisplayDialog = true)
        {
            checkResults?.Clear();
            nullResultCount = 0;

            if (!assetProvider)
            {
                string errorMessage = $"Asset checker execution failed. Argument '{nameof(assetProvider)}' is null.";
                UDebug.LogError($"[{LogTag}] {errorMessage}", logContext);
                if (allowDisplayDialog)
                {
                    EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                }
                return ExecutionResult.Failure;
            }

            if (assetCheckers == null || assetCheckers.Count == 0)
            {
                string errorMessage = $"Asset checker execution failed. Argument '{nameof(assetCheckers)}' is null or empty.";
                if (allowDisplayDialog)
                {
                    UDebug.LogError($"[{LogTag}] {errorMessage}", logContext);
                }
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                return ExecutionResult.Failure;
            }

            if (checkResults == null)
            {
                string errorMessage = $"Asset checker execution failed. Argument '{nameof(checkResults)}' is null.";
                if (allowDisplayDialog)
                {
                    UDebug.LogError($"[{LogTag}] {errorMessage}", logContext);
                }
                EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                return ExecutionResult.Failure;
            }

            IReadOnlyList<UObject> assets = assetProvider.GetAssets();
            if (assets == null || assets.Count == 0)
            {
                string errorMessage = "Asset checker execution aborted. No assets were provided for check.";
                UDebug.LogWarning($"[{LogTag}] {errorMessage}", logContext);
                if (allowDisplayDialog)
                {
                    EditorUtility.DisplayDialog("Warning", errorMessage, "Ok");
                }
                return ExecutionResult.NoAssets;
            }

            bool hasNullChecker = false;
            for (int i = 0; i < assets.Count; i++)
            {
                UObject asset = assets[i];
                for (int j = 0; j < assetCheckers.Count; j++)
                {
                    AssetChecker checker = assetCheckers[j];
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
                            checkResults.Add(result);
                        }
                        else
                        {
                            nullResultCount++;
                        }
                    }
                    catch (Exception e)
                    {
                        AssetCheckResult result = new AssetCheckResult
                        {
                            type = CheckResultType.Exception,
                            categories = new string[] { CategoryName_Exception },
                            title = e.GetType().Name,
                            details = e.Message,
                            asset = asset,
                            checker = checker,
                            repairable = false,
                            customData = null,
                            customViewId = null,
                        };
                        checkResults.Add(result);
                    }
                }
            }

            if (hasNullChecker)
            {
                string errorMessage = $"Null items found in the argument '{nameof(assetCheckers)}', please check.";
                UDebug.LogError($"[{LogTag}] {errorMessage}", logContext);
                if (allowDisplayDialog)
                {
                    EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                }
            }

            return ExecutionResult.Success;
        }
    }
}