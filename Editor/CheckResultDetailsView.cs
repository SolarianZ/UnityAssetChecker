using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

namespace GBG.AssetChecker.Editor.AssetChecker
{
    public delegate void AssetRecheckedHandler(AssetCheckResult newResult, AssetCheckResult oldResult);
    public delegate void AssetRepairedHandler(AssetCheckResult result, bool allIssuesRepaired);

    public class CheckResultDetailsView : VisualElement
    {
        private readonly Label _titleLabel;
        private readonly Label _typeLabel;
        private readonly Label _categoriesLabel;
        private readonly ObjectView _assetView;
        private readonly ObjectView _checkerView;
        private readonly ScrollView _detailsScrollView;
        private readonly Label _detailsLabel;
        private readonly Button _recheckButton;
        private readonly Button _repairButton;
        private AssetCheckResult _selectedResult;
        private CustomDetailsView _customDetailsView;

        public event AssetRecheckedHandler AssetRechecked;
        public event AssetRepairedHandler AssetRepaired;


        public CheckResultDetailsView()
        {
            style.flexGrow = 1;

            _titleLabel = new Label
            {
                name = "TitleLabel",
                style =
                {
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 4,
                    marginBottom = 4,
                    fontSize = 15,
                    unityTextAlign = TextAnchor.MiddleLeft,
                }
            };
#if UNITY_2022_3_OR_NEWER
            ((ITextSelection)_titleLabel).isSelectable = true; 
#endif
            Add(_titleLabel);

            VisualElement typeContainer = new VisualElement
            {
                name = "LabelContainer",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 4,
                    marginBottom = 4,
                    overflow = Overflow.Hidden,
                }
            };
            Add(typeContainer);

            _typeLabel = new Label
            {
                name = "TypeLabel",
                text = "-",
                style =
                {
                    marginRight = 4,
                    paddingLeft= 2,
                    paddingRight= 2,
                    borderLeftWidth = 2,
                    borderRightWidth = 2,
                    borderTopWidth = 2,
                    borderBottomWidth = 2,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
#if UNITY_2022_3_OR_NEWER
            ((ITextSelection)_typeLabel).isSelectable = true; 
#endif
            typeContainer.Add(_typeLabel);

            _categoriesLabel = new Label
            {
                name = "CategoriesLabel",
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    whiteSpace = WhiteSpace.Normal,
                }
            };
#if UNITY_2022_3_OR_NEWER
            ((ITextSelection)_categoriesLabel).isSelectable = true; 
#endif
            typeContainer.Add(_categoriesLabel);

            _assetView = new ObjectView(this, false)
            {
                name = "AssetView",
                style =
                {
                    marginLeft = 4,
                    marginRight = 4,
                    marginBottom = 1,
                }
            };
            Add(_assetView);

            _checkerView = new ObjectView(this, true)
            {
                name = "CheckerView",
                style =
                {
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 1,
                }
            };
            Add(_checkerView);

            _detailsScrollView = new ScrollView
            {
                name = "DetailsScrollView",
                style =
                {
                    flexGrow = 1,
                    marginTop = 8,
                }
            };
            Add(_detailsScrollView);

            _detailsLabel = new Label
            {
                name = "SelectableDetailsLabel",
                enableRichText = true,
                style =
                {
                    whiteSpace = WhiteSpace.Normal,
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 4,
                    marginBottom = 4,
                }
            };
#if UNITY_2022_3_OR_NEWER
            ((ITextSelection)_detailsLabel).isSelectable = true; 
#endif
            _detailsScrollView.contentContainer.Add(_detailsLabel);

            const float ButtonHeight = 28;
            VisualElement buttonContainer = new VisualElement
            {
                name = "ButtonContainer",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexEnd,
                    height = ButtonHeight,
                    minHeight = ButtonHeight,
                    maxHeight = ButtonHeight,
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 4,
                    marginBottom = 4,
                    paddingRight = 8,
                },
            };
            Add(buttonContainer);

            _recheckButton = new Button(RecheckAsset)
            {
                name = "RecheckButton",
                text = "Recheck",
            };
            _recheckButton.SetEnabled(false);
            buttonContainer.Add(_recheckButton);

            _repairButton = new Button(RepairAsset)
            {
                name = "RepairButton",
                text = "Try Repair",
            };
            _repairButton.SetEnabled(false);
            buttonContainer.Add(_repairButton);
        }

        public void SelectResult(AssetCheckResult selectedResult)
        {
            _selectedResult = selectedResult;
            if (_selectedResult == null)
            {
                ClearSelection();
                return;
            }

            UpdateResultTypeBorderColor(_selectedResult.type);
            _titleLabel.text = _selectedResult.title;
            _typeLabel.text = ObjectNames.NicifyVariableName(_selectedResult.type.ToString());
            _categoriesLabel.text = FormatCategories(_selectedResult.categories);
            _assetView.UpdateView();
            _checkerView.UpdateView();
            UpdateDetailsView();
            _recheckButton.SetEnabled(_selectedResult.asset && _selectedResult.checker);
            _repairButton.SetEnabled(_selectedResult.repairable);
        }

        public void ClearSelection()
        {
            _selectedResult = null;

            UpdateResultTypeBorderColor(CheckResultType.NotImportant);
            _typeLabel.text = "-";
            _categoriesLabel.text = null;
            _titleLabel.text = "-";
            _detailsScrollView.contentContainer.Clear();
            _detailsLabel.text = "-";
            _assetView.UpdateView();
            _checkerView.UpdateView();
            _recheckButton.SetEnabled(false);
            _repairButton.SetEnabled(false);
        }

        private string FormatCategories(string[] categories)
        {
            if (categories == null || categories.Length == 0)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder(categories[0].Length);
            builder.Append(categories[0]);

            for (int i = 1; i < categories.Length; i++)
            {
                string category = categories[i];
                if (string.IsNullOrEmpty(category))
                {
                    continue;
                }

                builder.Append(" | ").Append(category.Trim());
            }

            return builder.ToString();
        }

        private void UpdateResultTypeBorderColor(CheckResultType resultType)
        {
            Color color = resultType.GetResultTypeBorderColor();
            _typeLabel.style.borderLeftColor = color;
            _typeLabel.style.borderRightColor = color;
            _typeLabel.style.borderTopColor = color;
            _typeLabel.style.borderBottomColor = color;
        }

        private void UpdateDetailsView()
        {
            _detailsScrollView.contentContainer.Clear();

            if (_customDetailsView != null &&
                _customDetailsView.CustomViewId == _selectedResult.customViewId)
            {
                _customDetailsView.Bind(_selectedResult);
                _detailsScrollView.contentContainer.Add(_customDetailsView);
                return;
            }

            _customDetailsView = null;
            CustomViewProvider customViewProvider = AssetCheckerLocalCache.instance.InstantCustomViewProvider;
            if (!string.IsNullOrEmpty(_selectedResult.customViewId) && customViewProvider)
            {
                _customDetailsView = customViewProvider.GetDetailsView(_selectedResult.customViewId);
                if (_customDetailsView != null)
                {
                    Debug.Assert(_customDetailsView.CustomViewId == _selectedResult.customViewId);
                    _customDetailsView.Bind(_selectedResult);
                    _detailsScrollView.contentContainer.Add(_customDetailsView);
                    return;
                }

                Debug.LogError($"[{AssetCheckerWindow.LogTag}] Can not find custom details view of id {_selectedResult.customViewId}",
                    customViewProvider);
            }

            _detailsLabel.text = _selectedResult.details;
            _detailsScrollView.contentContainer.Add(_detailsLabel);
        }

        private void RecheckAsset()
        {
            AssetCheckResult oldResult = _selectedResult;
            AssetCheckResult newResult;
            try
            {
                newResult = oldResult.checker.CheckAsset(oldResult.asset);
            }
            catch (Exception e)
            {
                newResult = new AssetCheckResult
                {
                    type = CheckResultType.Exception,
                    categories = new string[] { "Exception" },
                    title = e.GetType().Name,
                    details = e.Message,
                    asset = oldResult.asset,
                    checker = oldResult.checker,
                    repairable = false,
                    customData = oldResult.customData,
                    customViewId = null,
                };
            }

            SelectResult(newResult);

            AssetRechecked?.Invoke(newResult, oldResult);
        }

        private void RepairAsset()
        {
            try
            {
                _selectedResult.checker.RepairAsset(_selectedResult, out bool allIssuesRepaired);
                if (allIssuesRepaired)
                {
                    AssetRepaired?.Invoke(_selectedResult, true);
                }
                else
                {
                    AssetRepaired?.Invoke(_selectedResult, false);
                }
            }
            catch (Exception e)
            {
                _selectedResult.type = CheckResultType.Exception;
                _selectedResult.title = e.GetType().Name;
                _selectedResult.details = e.Message;
                _selectedResult.repairable = false;

                SelectResult(_selectedResult);

                AssetRepaired?.Invoke(_selectedResult, false);
            }
        }


        public class ObjectView : VisualElement
        {
            private readonly Button _objectIconButton;
            private readonly Label _objectPathLabel;
            private readonly CheckResultDetailsView _owner;
            private readonly bool _isCheckerView;


            public ObjectView(CheckResultDetailsView owner, bool isCheckerView)
            {
                _owner = owner;
                _isCheckerView = isCheckerView;
                style.flexDirection = FlexDirection.Row;
                style.flexWrap = Wrap.Wrap;

                Label objectLabel = new Label
                {
                    name = "ObjectLabel",
                    text = isCheckerView ? "Checker" : "Asset",
                    style =
                    {
                        width = 52,
                        minWidth = 52,
                        maxWidth = 52,
                        paddingLeft = 1,
                        paddingRight = 1,
                        marginRight = 3,
                        unityTextAlign = TextAnchor.MiddleRight,
                    },
                };
                Add(objectLabel);

                const float ObjectIconSize = 20;
                _objectIconButton = new Button(PingAsset)
                {
                    name = "ObjectIconButton",
                    tooltip = "Ping object",
                    style =
                    {
                        alignSelf = Align.Center,
                        width = ObjectIconSize,
                        minWidth = ObjectIconSize,
                        maxWidth = ObjectIconSize,
                        height = ObjectIconSize,
                        minHeight = ObjectIconSize,
                        maxHeight = ObjectIconSize,
                        paddingLeft = 1,
                        paddingRight = 1,
                        paddingTop = 1,
                        paddingBottom = 1,
                        marginLeft = 1,
                        marginRight = 1,
                        marginTop = 1,
                        marginBottom = 1,
                    }
                };
                Add(_objectIconButton);

                _objectPathLabel = new Label
                {
                    name = "ObjectPathLabel",
                    text = "-",
                    style =
                    {
                        flexGrow = 1,
                        flexShrink = 1,
                        marginLeft = 3,
                        paddingLeft = 1,
                        paddingRight = 1,
                        overflow = Overflow.Hidden,
                        unityFontStyleAndWeight = FontStyle.Italic,
                        unityTextAlign = TextAnchor.MiddleLeft,
                        whiteSpace = WhiteSpace.Normal,
                    }
                };
#if UNITY_2022_3_OR_NEWER
                ((ITextSelection)_objectPathLabel).isSelectable = true; 
#endif
                Add(_objectPathLabel);
            }

            public void UpdateView()
            {
                UObject target = null;
                if (_owner._selectedResult != null)
                {
                    target = _isCheckerView
                        ? _owner._selectedResult.checker
                        : _owner._selectedResult.asset;
                }
                if (target)
                {
                    string targetPath = AssetDatabase.GetAssetPath(target);
                    _objectPathLabel.text = targetPath;
                    _objectIconButton.style.backgroundImage = new StyleBackground(AssetPreview.GetMiniThumbnail(target));
                    //_objectIconButton.style.backgroundImage = new StyleBackground(AssetDatabase.GetCachedIcon(targetPath) as Texture2D);
                }
                else
                {
                    _objectPathLabel.text = "-";
                    _objectIconButton.style.backgroundImage = null;
                }
            }

            private void PingAsset()
            {
                if (_owner._selectedResult != null)
                {
                    UObject target = _isCheckerView
                        ? _owner._selectedResult.checker
                        : _owner._selectedResult.asset;
                    if (target)
                    {
                        EditorGUIUtility.PingObject(target);
                    }
                }
            }
        }
    }
}