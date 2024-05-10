using System;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecker.Editor.AssetChecker
{
    [Serializable]
    public class AssetCheckResult
    {
        public CheckResultType type = CheckResultType.Exception;
        public string[] categories;
        public string title;
        public string details;
        public UObject asset;
        public AssetChecker checker;
        public bool repairable;
        [SerializeReference]
        public AssetCheckResultCustomData customData;
        public string customViewId;
    }

    [Serializable]
    public abstract class AssetCheckResultCustomData
    {

    }
}