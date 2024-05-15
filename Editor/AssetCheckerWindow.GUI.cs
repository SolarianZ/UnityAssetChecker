using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GBG.AssetChecking.Editor
{
    public partial class AssetCheckerWindow
    {
        #region Controls

        private ObjectField _settingsField;
        private HelpBox _executionHelpBox;
        private Button _executeButton;
        private Label _resultStatsLabel;
        private EnumFlagsField _resultTypeFilterField;
        private PopupField<string> _resultCategoryFilterField; // DropdownField is not supported in Unity 2020
        private ListView _resultListView;
        private CheckResultDetailsView _resultDetailsView;

        private GUIContent _helpButtonContent;
        private GUIStyle _helpButtonStyle;

        #endregion


        private void ShowButton(Rect position)
        {
            if (_helpButtonContent == null)
            {
                _helpButtonStyle = GUI.skin.FindStyle("IconButton");
            }

            if (GUI.Button(position, EditorGUIUtility.IconContent("_Help"), GUI.skin.FindStyle("IconButton")))
            {
                Application.OpenURL("https://github.com/SolarianZ/UnityAssetChecker");
            }
        }

        private void CreateGUI()
        {
            minSize = new Vector2(500, 400);
            VisualElement root = rootVisualElement;


            #region Toolbar

            // Toolbar
            Toolbar toolbar = new Toolbar
            {
                name = "Toolbar",
                style =
                {
                    justifyContent = Justify.SpaceBetween,
                }
            };
            root.Add(toolbar);

            VisualElement styleContainer = new VisualElement
            {
                name = "StyleContainer",
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };
            toolbar.Add(styleContainer);

            // Type Icon Style
            ToolbarMenu typeIconStyleMenu = new ToolbarMenu
            {
                name = "TypeIconStyleMenu",
                text = GetResultTypeIconStyleName(LocalCache.GetCheckResultTypeIconStyle())
            };
            InitializeTypeIconStyleMenu(typeIconStyleMenu);
            styleContainer.Add(typeIconStyleMenu);

            // Asset Icon
            ToolbarToggle assetIconToggle = new ToolbarToggle
            {
                name = "AssetIconToggle",
                text = "Show Asset Icon",
                value = LocalCache.GetShowResultEntryAssetIcon(),
            };
            assetIconToggle.RegisterValueChangedCallback(OnShowResultEntryAssetIconChanged);
            styleContainer.Add(assetIconToggle);

            // Clear Button
            ToolbarButton clearResultsButton = new ToolbarButton(ClearCheckResults)
            {
                name = "ClearResultsButton",
                text = "Clear Check Results",
            };
            toolbar.Add(clearResultsButton);

            #endregion


            #region Settings

            // Settings Container
            VisualElement settingsContainer = new VisualElement
            {
                name = "SettingsContainer",
                style =
                {
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 4,
                    paddingBottom = 4,
                }
            };
            root.Add(settingsContainer);

            // Asset Container
            VisualElement settingsAssetContainer = new VisualElement
            {
                name = "SettingsAssetContainer",
                style =
                {
                    flexDirection = FlexDirection.Row,
                }
            };
            settingsContainer.Add(settingsAssetContainer);

            // UObject Field
            _settingsField = new ObjectField
            {
                name = "SettingsField",
                label = "Settings",
                allowSceneObjects = false,
                objectType = typeof(AssetCheckerSettings),
                value = _settings,
                style =
                {
                    flexGrow = 1,
                }
            };
            _settingsField.RegisterValueChangedCallback(OnSettingsObjectChanged);
            settingsAssetContainer.Add(_settingsField);

            // Create Button
            Button createSettingsAssetButton = new Button
            {
                name = "CreateSettingsAssetButton",
                text = "New",
            };
            createSettingsAssetButton.clicked += () =>
            {
                _settings = CreateSettingsAsset();
            };
            settingsAssetContainer.Add(createSettingsAssetButton);

            #endregion


            #region Execution

            // Execution HelpBox
            _executionHelpBox = new HelpBox
            {
                name = "ExecutionHelpBox",
                messageType = HelpBoxMessageType.Error,
                style =
                {
                    marginLeft = 16,
                    marginRight = 16,
                }
            };
            _executionHelpBox.Q<Label>().style.fontSize = 13;
            root.Add(_executionHelpBox);

            // Execution Button
            _executeButton = new Button(Execute)
            {
                name = "ExecuteButton",
                text = "Execute",
                style =
                {
                    height = 28,
                    marginLeft = 8,
                    marginRight = 8,
                    marginTop = 8,
                    marginBottom = 8,
                }
            };
            root.Add(_executeButton);

            #endregion

            // Separator
            root.Add(new VisualElement
            {
                name = "SettingsSeparator",
                style =
                {
                    backgroundColor = EditorGUIUtility.isProSkin
                        ? new Color(26 / 255f, 26 / 255f, 26 / 255f, 1.0f)
                        : new Color(127 / 255f, 127 / 255f, 127 / 255f, 1.0f),
                    width = StyleKeyword.Auto,
                    height = 1,
                    minHeight = 1,
                    maxHeight = 1,
                    marginLeft = 16,
                    marginRight = 16,
                }
            });

            // Result Stats
            _resultStatsLabel = new Label
            {
                name = "ResultStatsLabel",
                style =
                {
                    fontSize = 12,
                    marginLeft = 16,
                    marginRight = 16,
                    marginTop = 4,
                    marginBottom = 4,
                    whiteSpace = WhiteSpace.Normal,
                }
            };
            root.Add(_resultStatsLabel);

            // Result Container
            TwoPaneSplitView resultContainer = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal)
            {
                name = "ResultContainer",
            };
            resultContainer.RegisterCallback<GeometryChangedEvent>(SetupResultPanes);
            root.Add(resultContainer);

            #region Result List

            VisualElement resultListContainer = new VisualElement
            {
                name = "ResultListContainer",
            };
            resultContainer.Add(resultListContainer);

            // Type Filter
            _resultTypeFilterField = new EnumFlagsField(LocalCache.GetCheckResultTypeFilter())
            {
                name = "ResultTypeFilterField",
            };
            _resultTypeFilterField.RegisterValueChangedCallback(OnResultTypeFilterChanged);
            resultListContainer.Add(_resultTypeFilterField);

            // Category Filter
            _resultCategoryFilterField = new PopupField<string>
            {
                name = "ResultCategoryFilterField",
                choices = _resultCategories,
                value = LocalCache.GetCheckResultCategoryFilter(),
            };
            _resultCategoryFilterField.RegisterValueChangedCallback(OnResultCategoryFilterChanged);
            resultListContainer.Add(_resultCategoryFilterField);

            // Result List View
            _resultListView = new ListView
            {
                name = "ResultListView",
                itemsSource = _filteredCheckResults,
#if UNITY_2021_3_OR_NEWER
                fixedItemHeight = 28,
#else
                itemHeight = 28,
#endif
                selectionType = SelectionType.Single,
                makeItem = MakeResultListItem,
                bindItem = BindResultListItem,
                style =
                {
                    flexGrow = 1, // Fix display issue on Unity 2020
                }
            };
#if UNITY_2022_3_OR_NEWER
            _resultListView.selectionChanged += OnCheckResultSelectionChanged;
#else
            _resultListView.onSelectionChange += OnCheckResultSelectionChanged;
#endif
            resultListContainer.Add(_resultListView);

            #endregion


            #region Result Details

            // Details View
            _resultDetailsView = new CheckResultDetailsView
            {
                name = "CheckResultDetailsView",
            };
            _resultDetailsView.AssetRechecked += OnAssetRechecked;
            _resultDetailsView.AssetRepaired += OnAssetRepaired;
            resultContainer.Add(_resultDetailsView);

            #endregion


            // SelectResult properties - Not work on Unity 2020
            //_settingsField.bindingPath = nameof(_settings); 
            //root.Bind(new SerializedObject(this));

            // Restore values
            UpdateExecutionControls();
            UpdateResultControls(true);
        }

        // Fix fixedPane null exception on Unity 2021
        private void SetupResultPanes(GeometryChangedEvent evt)
        {
            TwoPaneSplitView resultContainer = (TwoPaneSplitView)evt.target;
            resultContainer.UnregisterCallback<GeometryChangedEvent>(SetupResultPanes);

            resultContainer.schedule.Execute(() =>
            {
                resultContainer.fixedPane.style.minWidth = 200;
                resultContainer.flexedPane.style.minWidth = 200;
            });
        }

        private void UpdateExecutionControls()
        {
            if (_executionHelpBox == null || _executeButton == null)
            {
                return;
            }

            if (!_settings)
            {
                _executionHelpBox.messageType = HelpBoxMessageType.Error;
                _executionHelpBox.text = "Please specify settings asset.";
                _executionHelpBox.style.display = DisplayStyle.Flex;
                _executeButton.SetEnabled(false);
                return;
            }

            if (!_settings.assetProvider)
            {
                _executionHelpBox.messageType = HelpBoxMessageType.Error;
                _executionHelpBox.text = "No asset provider specified in the settings.";
                _executionHelpBox.style.display = DisplayStyle.Flex;
                _executeButton.SetEnabled(false);
                return;
            }

            AssetChecker[] checkers = _settings.assetCheckers;
            if (checkers == null || checkers.Length == 0)
            {
                _executionHelpBox.messageType = HelpBoxMessageType.Error;
                _executionHelpBox.text = "No asset checker specified in the settings.";
                _executionHelpBox.style.display = DisplayStyle.Flex;
                _executeButton.SetEnabled(false);
                return;
            }

            for (int i = 0; i < checkers.Length; i++)
            {
                AssetChecker checker = checkers[i];
                if (!checker)
                {
                    _executionHelpBox.messageType = HelpBoxMessageType.Warning;
                    _executionHelpBox.text = "There are null asset checkers in the settings, please check.";
                    _executionHelpBox.style.display = DisplayStyle.Flex;
                    return;
                }
            }

            _executionHelpBox.messageType = HelpBoxMessageType.Error;
            _executionHelpBox.text = "You should not see this message.";
            _executionHelpBox.style.display = DisplayStyle.None;
            _executeButton.SetEnabled(true);
        }

        private void UpdateResultControls(bool clearSelection)
        {
            if (_resultStatsLabel == null ||
                _resultListView == null ||
                _resultDetailsView == null)
            {
                return;
            }

            _resultStatsLabel.text = $"Total: {_stats.GetTotal(false)}  Filtered: {_filteredCheckResults.Count}  " +
               $"Error: {_stats.error}  Warning: {_stats.warning}  Not Important: {_stats.notImportant}  " +
               $"All Pass: {_stats.allPass}  Exception: {_stats.exception}  (Null Result: {_stats.nullResult})";
            RefreshResultListView();

            if (clearSelection)
            {
                _resultListView.ClearSelection();
                _resultDetailsView.ClearSelection();
            }
        }

        private VisualElement MakeResultListItem()
        {
            CheckResultEntryView item = new CheckResultEntryView();
            return item;
        }

        private void BindResultListItem(VisualElement element, int index)
        {
            AssetCheckResult result = _filteredCheckResults[index];
            CheckResultEntryView item = (CheckResultEntryView)element;
            item.Bind(result);
        }

        private void OnCheckResultSelectionChanged(IEnumerable<object> selectedItems)
        {
            AssetCheckResult result = (AssetCheckResult)_resultListView.selectedItem;
            _resultDetailsView.SelectResult(result);
        }

        private void RefreshResultListView()
        {
#if UNITY_2021_3_OR_NEWER
            _resultListView.Rebuild();
#else
            _resultListView.Refresh();
#endif
        }

        private void InitializeTypeIconStyleMenu(ToolbarMenu menu)
        {
            ResultTypeIconStyle currentStyle = LocalCache.GetCheckResultTypeIconStyle();

            menu.menu.AppendAction(GetResultTypeIconStyleName(ResultTypeIconStyle.Style1), _ =>
            {
                LocalCache.SetCheckResultIconStyle(ResultTypeIconStyle.Style1);
                menu.text = GetResultTypeIconStyleName(ResultTypeIconStyle.Style1);
                RefreshResultListView();
            }, GetResultTypeIconStyleMenuItemStatus, ResultTypeIconStyle.Style1);
            menu.menu.AppendAction(GetResultTypeIconStyleName(ResultTypeIconStyle.Style2), _ =>
            {
                LocalCache.SetCheckResultIconStyle(ResultTypeIconStyle.Style2);
                menu.text = GetResultTypeIconStyleName(ResultTypeIconStyle.Style2);
                RefreshResultListView();
            }, GetResultTypeIconStyleMenuItemStatus, ResultTypeIconStyle.Style2);
            menu.menu.AppendAction(GetResultTypeIconStyleName(ResultTypeIconStyle.Style3), _ =>
            {
                LocalCache.SetCheckResultIconStyle(ResultTypeIconStyle.Style3);
                menu.text = GetResultTypeIconStyleName(ResultTypeIconStyle.Style3);
                RefreshResultListView();
            }, GetResultTypeIconStyleMenuItemStatus, ResultTypeIconStyle.Style3);
        }

        private static string GetResultTypeIconStyleName(ResultTypeIconStyle iconStyle)
        {
            switch (iconStyle)
            {
                case ResultTypeIconStyle.Style1:
                    return "Type Icon Style 1";
                case ResultTypeIconStyle.Style2:
                    return "Type Icon Style 2";
                case ResultTypeIconStyle.Style3:
                    return "Type Icon Style 3";
                default:
                    throw new ArgumentOutOfRangeException(nameof(iconStyle), iconStyle, null);
            }
        }

        private static DropdownMenuAction.Status GetResultTypeIconStyleMenuItemStatus(DropdownMenuAction action)
        {
            ResultTypeIconStyle currentStyle = LocalCache.GetCheckResultTypeIconStyle();
            if (((ResultTypeIconStyle)action.userData) == currentStyle)
            {
                return DropdownMenuAction.Status.Checked | DropdownMenuAction.Status.Disabled;
            }

            return DropdownMenuAction.Status.Normal;
        }
    }
}