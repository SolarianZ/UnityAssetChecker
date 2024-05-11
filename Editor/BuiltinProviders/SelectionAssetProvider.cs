using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecking.Editor
{
    [CreateAssetMenu(menuName = "Bamboo/Asset Checker/Selection Asset Provider")]
    public class SelectionAssetProvider : AssetProvider
    {
        public SelectionMode selectionModes = SelectionMode.Unfiltered;


        public override IReadOnlyList<UObject> GetAssets()
        {
            UObject[] objects = Selection.GetFiltered<UObject>(selectionModes);
            return objects;
        }
    }
}