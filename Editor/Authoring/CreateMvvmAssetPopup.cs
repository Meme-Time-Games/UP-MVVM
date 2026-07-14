using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MVVM.Core;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM.CoreEditor
{
    public class CreateMvvmAssetPopup : EditorWindow
    {
        private const string DefaultFolderPath = "Assets";
        private const string StyleSheetPath = "Packages/com.mtg.mvvm/Editor/Explorer/MvvmExplorerStyles.uss";

        private readonly MvvmAssetFactory _mvvmAssetFactory = new MvvmAssetFactory();
        private readonly MvvmAssetRepository _mvvmAssetRepository = new MvvmAssetRepository();
        private readonly DuplicateNameFinder _duplicateNameFinder = new DuplicateNameFinder();
        private readonly MvvmAssetNameBuilder _mvvmAssetNameBuilder = new MvvmAssetNameBuilder();
        private readonly AdvancedDropdownState _typeDropdownState = new AdvancedDropdownState();

        private IReadOnlyList<Type> _reactiveVariableTypes;
        private List<string> _existingAssetNames;
        private ToolbarToggle _eventToggle;
        private ToolbarToggle _reactiveVariableToggle;
        private VisualElement _typeRow;
        private Button _typeButton;
        private TextField _nameField;
        private Label _previewLabel;
        private TextField _folderField;
        private Label _warningLabel;
        private Type _selectedReactiveVariableType;
        private bool _isEventKind;

        public Action OnCreated { get; set; }

        public static void ShowPopup(Action onCreated)
        {
            CreateMvvmAssetPopup popup = CreateInstance<CreateMvvmAssetPopup>();
            popup.titleContent = new GUIContent("Create MVVM Asset");
            popup.OnCreated = onCreated;
            popup.ShowUtility();
        }

        private void CreateGUI()
        {
            _reactiveVariableTypes = _mvvmAssetFactory.GetCreatableTypes()
                .Where(creatableType => !typeof(EventViewModelSO).IsAssignableFrom(creatableType))
                .ToList();
            _existingAssetNames = _mvvmAssetRepository.GetAllAssets()
                .Select(asset => asset.Name)
                .ToList();
            _selectedReactiveVariableType = _reactiveVariableTypes.FirstOrDefault();

            ApplyStyleSheet();

            rootVisualElement.Add(CreateKindRow());

            _typeRow = CreateTypeRow();
            rootVisualElement.Add(_typeRow);

            _nameField = new TextField("Name");
            _nameField.RegisterValueChangedCallback(nameChange => RefreshPreview());
            rootVisualElement.Add(_nameField);

            _previewLabel = new Label();
            rootVisualElement.Add(_previewLabel);

            rootVisualElement.Add(CreateFolderRow());

            _warningLabel = new Label();
            _warningLabel.style.color = Color.yellow;
            rootVisualElement.Add(_warningLabel);

            Button createButton = new Button(CreateAsset);
            createButton.text = "Create";
            rootVisualElement.Add(createButton);

            RefreshPreview();
        }

        private VisualElement CreateKindRow()
        {
            Toolbar kindRow = new Toolbar();

            _reactiveVariableToggle = new ToolbarToggle();
            _reactiveVariableToggle.text = "Reactive Variable";
            _reactiveVariableToggle.SetValueWithoutNotify(true);
            _reactiveVariableToggle.RegisterValueChangedCallback(SwitchToReactiveVariableKind);
            kindRow.Add(_reactiveVariableToggle);

            _eventToggle = new ToolbarToggle();
            _eventToggle.text = "Event";
            _eventToggle.RegisterValueChangedCallback(SwitchToEventKind);
            kindRow.Add(_eventToggle);

            return kindRow;
        }

        private void SwitchToReactiveVariableKind(ChangeEvent<bool> changeEvent)
        {
            if (!changeEvent.newValue)
            {
                _reactiveVariableToggle.SetValueWithoutNotify(true);
                return;
            }

            _eventToggle.SetValueWithoutNotify(false);
            _isEventKind = false;
            _typeRow.RemoveFromClassList("is-hidden");
            RefreshPreview();
        }

        private void SwitchToEventKind(ChangeEvent<bool> changeEvent)
        {
            if (!changeEvent.newValue)
            {
                _eventToggle.SetValueWithoutNotify(true);
                return;
            }

            _reactiveVariableToggle.SetValueWithoutNotify(false);
            _isEventKind = true;
            _typeRow.AddToClassList("is-hidden");
            RefreshPreview();
        }

        private VisualElement CreateTypeRow()
        {
            VisualElement typeRow = new VisualElement();
            typeRow.style.flexDirection = FlexDirection.Row;

            Label typeLabel = new Label("Type");
            typeRow.Add(typeLabel);

            _typeButton = new Button(OpenTypeDropdown);
            RefreshTypeButtonText();
            typeRow.Add(_typeButton);

            return typeRow;
        }

        private void OpenTypeDropdown()
        {
            MvvmTypeDropdown typeDropdown =
                new MvvmTypeDropdown(_typeDropdownState, _reactiveVariableTypes, SelectReactiveVariableType);

            typeDropdown.Show(_typeButton.worldBound);
        }

        private void SelectReactiveVariableType(Type selectedType)
        {
            _selectedReactiveVariableType = selectedType;
            RefreshTypeButtonText();
            RefreshPreview();
        }

        private void RefreshTypeButtonText()
        {
            _typeButton.text = _selectedReactiveVariableType.Name;
        }

        private VisualElement CreateFolderRow()
        {
            _folderField = new TextField("Folder");
            _folderField.SetValueWithoutNotify(GetSelectedFolderPath());
            _folderField.isReadOnly = true;
            _folderField.style.flexGrow = 1;

            Button browseButton = new Button(BrowseFolder);
            browseButton.text = "Browse";

            VisualElement folderRow = new VisualElement();
            folderRow.style.flexDirection = FlexDirection.Row;
            folderRow.Add(_folderField);
            folderRow.Add(browseButton);

            return folderRow;
        }

        private void BrowseFolder()
        {
            string absolutePath = EditorUtility.OpenFolderPanel("Select folder", _folderField.value, string.Empty);

            if (string.IsNullOrEmpty(absolutePath))
                return;

            string projectPath = GetProjectRelativePath(absolutePath);

            if (string.IsNullOrEmpty(projectPath))
            {
                ShowError("The folder must be inside the project.");
                return;
            }

            _folderField.SetValueWithoutNotify(projectPath);
            RefreshPreview();
        }

        private string GetProjectRelativePath(string absolutePath)
        {
            string normalizedPath = absolutePath.Replace("\\", "/");
            string dataPath = Application.dataPath;

            if (!normalizedPath.StartsWith(dataPath, StringComparison.Ordinal))
                return string.Empty;

            return "Assets" + normalizedPath.Substring(dataPath.Length);
        }

        private void RefreshPreview()
        {
            string name = _nameField.value;

            if (string.IsNullOrWhiteSpace(name))
            {
                _previewLabel.text = string.Empty;
                _warningLabel.style.color = Color.yellow;
                _warningLabel.text = string.Empty;
                return;
            }

            string finalName = _mvvmAssetNameBuilder.GetAssetNameWithTypeNameAndName(GetSelectedType().Name, name);

            _previewLabel.text = $"{finalName}.asset in {_folderField.value}";
            _warningLabel.style.color = Color.yellow;
            _warningLabel.text = GetSimilarNameWarningWithName(finalName);
        }

        private string GetSimilarNameWarningWithName(string finalName)
        {
            IReadOnlyList<string> similarNames =
                _duplicateNameFinder.GetSimilarNamesWithName(finalName, _existingAssetNames);

            if (similarNames.Count == 0)
                return string.Empty;

            return $"Similar asset already exists: {string.Join(", ", similarNames)}";
        }

        private Type GetSelectedType()
        {
            if (_isEventKind)
                return typeof(EventViewModelSO);

            return _selectedReactiveVariableType;
        }

        private void CreateAsset()
        {
            if (string.IsNullOrWhiteSpace(_nameField.value))
            {
                ShowError("Enter a name for the asset.");
                return;
            }

            Type selectedType = GetSelectedType();
            string finalName = _mvvmAssetNameBuilder.GetAssetNameWithTypeNameAndName(selectedType.Name, _nameField.value);

            try
            {
                ScriptableObject createdAsset = _mvvmAssetFactory.Create(selectedType, finalName, _folderField.value);

                Selection.activeObject = createdAsset;
                EditorGUIUtility.PingObject(createdAsset);

                OnCreated?.Invoke();
                Close();
            }
            catch (DirectoryNotFoundException directoryNotFoundException)
            {
                ShowError(directoryNotFoundException.Message);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                ShowError(invalidOperationException.Message);
            }
        }

        private void ShowError(string message)
        {
            _warningLabel.style.color = Color.red;
            _warningLabel.text = message;
        }

        private string GetSelectedFolderPath()
        {
            if (Selection.activeObject == null)
                return DefaultFolderPath;

            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(selectedPath))
                return DefaultFolderPath;

            if (AssetDatabase.IsValidFolder(selectedPath))
                return selectedPath;

            return Path.GetDirectoryName(selectedPath).Replace("\\", "/");
        }

        private void ApplyStyleSheet()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);

            if (styleSheet == null)
                return;

            rootVisualElement.styleSheets.Add(styleSheet);
        }
    }
}
