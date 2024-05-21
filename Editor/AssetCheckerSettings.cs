using System;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace GBG.AssetChecking.Editor
{
    [CreateAssetMenu(menuName = "Bamboo/Asset Checker/Asset Checker Settings")]
    public class AssetCheckerSettings : ScriptableObject
    {
#nullable enable
        [Tooltip("Provides custom views. Can be null.")]
        public CustomViewProvider? customViewProvider;
#nullable disable
#if ODIN_INSPECTOR
        [Required]
#endif
        [Tooltip("Provides assets for checking. Required.")]
        public AssetProvider assetProvider;
#if ODIN_INSPECTOR
        [Required]
        [RequiredListLength(1, null)]
#endif
        [Tooltip("Provides checkers for checking assets. Required.")]
        public AssetChecker[] assetCheckers = Array.Empty<AssetChecker>();

#if ODIN_INSPECTOR
        [Required]
        [RequiredListLength(1, null)]
#endif
        public AssetCheckerProfile[] profiles = Array.Empty<AssetCheckerProfile>();
    }

    [Serializable]
    public class AssetCheckerProfile
    {
#if ODIN_INSPECTOR
        [Required]
#endif
        [Tooltip("Name of the profile.")]
        public string name = DateTime.Now.ToString();

#nullable enable
        [Tooltip("Provides custom views. Can be null.")]
        public CustomViewProvider? customViewProvider;
#nullable disable
#if ODIN_INSPECTOR
        [Required]
#endif
        [Tooltip("Provides assets for checking. Required.")]
        public AssetProvider assetProvider;
#if ODIN_INSPECTOR
        [Required]
        [RequiredListLength(1, null)]
#endif
        [Tooltip("Provides checkers for checking assets. Required.")]
        public AssetChecker[] assetCheckers = Array.Empty<AssetChecker>();
    }
}