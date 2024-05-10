using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecker.Editor.AssetChecker
{
    [CreateAssetMenu(menuName = "Bamboo/Asset Checker/Explicit Asset Provider")]
    public class ExplicitAssetProvider : AssetProvider
    {
        public UObject[] assets = Array.Empty<UObject>();


        public override IReadOnlyList<UObject> GetAssets()
        {
            return assets;
        }
    }
}