using UnityEngine;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecking.Editor
{
    public abstract class AssetChecker : ScriptableObject
    {
        /// <summary>
        /// Execute the asset check process.
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
    }
}