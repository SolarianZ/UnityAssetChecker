using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.AssetChecking.Editor
{
    public abstract class CustomDetailsView : VisualElement
    {
        public abstract string CustomViewId { get; }


        public abstract void Bind(AssetCheckResult checkResult);
    }

    public abstract class CustomViewProvider : ScriptableObject
    {
        public abstract CustomDetailsView GetDetailsView(string customViewId);
    }
}