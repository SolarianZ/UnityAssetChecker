using System.Collections.Generic;
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
        [Tooltip("The filter string can provide names, labels or types (class names).")]
        public string filter;
        [Tooltip("The folders where the search will start.")]
        public string[] searchInFolders;


        public override IReadOnlyList<UObject> GetAssets()
        {
            string[] guids = AssetDatabase.FindAssets(filter, searchInFolders);
            List<UObject> objects = new List<UObject>(guids.Length);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UObject asset = AssetDatabase.LoadAssetAtPath<UObject>(path);
                objects.Add(asset);
            }
            return objects;
        }
    }
}