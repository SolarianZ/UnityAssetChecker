using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.AssetChecking.Editor
{
    public class CheckResultEntryView : VisualElement
    {
        private readonly Image _typeImage;
        private readonly Image _assetImage;
        private readonly Label _label;
        private readonly Image _repairableImage;


        public CheckResultEntryView()
        {
            style.flexDirection = FlexDirection.Row;

            _typeImage = new Image
            {
                name = "TypeImage",
                style =
                {
                    width = 20,
                    minWidth = 20,
                    maxWidth = 20,
                    height = 20,
                    minHeight = 20,
                    maxHeight = 20,
                    alignSelf = Align.Center,
                }
            };
            Add(_typeImage);

            _assetImage = new Image
            {
                name = "AssetImage",
                style =
                {
                    width = 20,
                    minWidth = 20,
                    maxWidth = 20,
                    height = 20,
                    minHeight = 20,
                    maxHeight = 20,
                    alignSelf = Align.Center,
                }
            };
            Add(_assetImage);

            _label = new Label
            {
                name = "Label",
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    overflow = Overflow.Hidden,
                    unityTextAlign = TextAnchor.MiddleLeft,
                }
            };
            Add(_label);

            _repairableImage = new Image
            {
                name = "RepairableImage",
                tooltip = "Repairable",
                image = EditorGUIUtility.isProSkin
                            ? EditorGUIUtility.IconContent("d_CustomTool@2x").image
                            : EditorGUIUtility.IconContent("CustomTool@2x").image,
                style =
                {
                    width = 20,
                    minWidth = 20,
                    maxWidth = 20,
                    height = 20,
                    minHeight = 20,
                    maxHeight = 20,
                    alignSelf = Align.Center,
                }
            };
            Add(_repairableImage);
        }

        public void Bind(AssetCheckResult result)
        {
            ResultTypeIconStyle iconStyle = AssetCheckerLocalCache.instance.GetCheckResultTypeIconStyle();
            _typeImage.image = result.type.GetResultTypeIcon(iconStyle);
            if (AssetCheckerLocalCache.instance.GetShowResultEntryAssetIcon())
            {
                _assetImage.style.display = DisplayStyle.Flex;
                _assetImage.image = result.asset != null
                    ? AssetPreview.GetMiniThumbnail(result.asset)
                    : null;
            }
            else
            {
                _assetImage.style.display = DisplayStyle.None;
            }
            _label.text = result.title;
            _repairableImage.style.display = result.repairable
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }
}