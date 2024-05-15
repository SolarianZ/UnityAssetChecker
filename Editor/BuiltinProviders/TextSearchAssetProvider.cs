using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecking.Editor
{
    /// <summary>
    /// Search the asset database using the search filter string.
    /// </summary>
    /// <seealso cref="AssetDatabase.FindAssets(string, string[])"/>
    [HelpURL("https://docs.unity3d.com/ScriptReference/AssetDatabase.FindAssets.html")]
    [CreateAssetMenu(menuName = "Bamboo/Asset Checker/Text Search Asset Provider")]
    public class TextSearchAssetProvider : AssetProvider
    {
        [Tooltip("The filter string can provide names, labels or types (class names). Refer to AssetDatabase.FindAssets.")]
        public string filter;
        [Tooltip("If not empty, the search results will only include assets whose paths match this regular expression.")]
        public string includePattern;
        [Tooltip("If not empty, the search results will not include assets whose paths match this regular expression.")]
        public string excludePattern;
        [Tooltip("The folders where the search will start. Refer to AssetDatabase.FindAssets.")]
        public string[] searchInFolders = new string[] { "Assets" };
        [Tooltip("If checked, the search results will not include DefaultAssets (folders, unsupported file types, etc.).")]
        public bool ignoreDefaultAssets;


        public override IReadOnlyList<UObject> GetAssets()
        {
            string[] guids = AssetDatabase.FindAssets(filter, searchInFolders);
            List<UObject> objects = new List<UObject>(guids.Length);
            Regex includeRegex = string.IsNullOrEmpty(includePattern) ? null : new Regex(includePattern);
            Regex excludeRegex = string.IsNullOrEmpty(excludePattern) ? null : new Regex(excludePattern);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (includeRegex != null && !includeRegex.IsMatch(path))
                {
                    continue;
                }

                if (excludeRegex != null && includeRegex.IsMatch(path))
                {
                    continue;
                }

                UObject asset = AssetDatabase.LoadAssetAtPath<UObject>(path);
                if (ignoreDefaultAssets && asset is DefaultAsset)
                {
                    continue;
                }

                objects.Add(asset);
            }
            return objects;
        }
    }
}