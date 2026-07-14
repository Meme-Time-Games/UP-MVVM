using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVVM.CoreEditor
{
    public class CreateMvvmAssetPopup : EditorWindow
    {
        private const string DefaultFolderPath = "Assets";

        private readonly MvvmAssetFactory _mvvmAssetFactory = new MvvmAssetFactory();
        private readonly MvvmAssetRepository _mvvmAssetRepository = new MvvmAssetRepository();
        private readonly DuplicateNameFinder _duplicateNameFinder = new DuplicateNameFinder();

        private IReadOnlyList<Type> _creatableTypes;
        private List<string> _existingAssetNames;
        private PopupField<string> _typeField;
        private TextField _nameField;
        private TextField _folderField;
        private Label _warningLabel;

        public static void ShowPopup(Action onCreated)
        {
            CreateMvvmAssetPopup popup = CreateInstance<CreateMvvmAssetPopup>();
            popup.titleContent = new GUIContent("Create MVVM Asset");
            popup.OnCreated = onCreated;
            popup.ShowUtility();
        }

        public Action OnCreated { get; set; }

        private void CreateGUI()
        {
            _creatableTypes = _mvvmAssetFactory.GetCreatableTypes();
            _existingAssetNames = _mvvmAssetRepository.GetAllAssets()
                .Select(asset => asset.Name)
                .ToList();

            List<string> typeNames = _creatableTypes.Select(creatableType => creatableType.Name).ToList();
            _typeField = new PopupField<string>("Type", typeNames, 0);

            _nameField = new TextField("Name");
            _nameField.RegisterValueChangedCallback(nameChange => RefreshWarning());

            _folderField = new TextField("Folder");
            _folderField.SetValueWithoutNotify(GetSelectedFolderPath());

            _warningLabel = new Label();
            _warningLabel.style.color = Color.yellow;

            Button createButton = new Button(CreateAsset);
            createButton.text = "Create";

            rootVisualElement.Add(_typeField);
            rootVisualElement.Add(_nameField);
            rootVisualElement.Add(_folderField);
            rootVisualElement.Add(_warningLabel);
            rootVisualElement.Add(createButton);
        }

        private void RefreshWarning()
        {
            _warningLabel.style.color = Color.yellow;
            _warningLabel.text = GetSimilarNameWarning();
        }

        private string GetSimilarNameWarning()
        {
            if (string.IsNullOrEmpty(_nameField.value))
                return string.Empty;

            List<string> names = new List<string>(_existingAssetNames) { _nameField.value };

            foreach (IReadOnlyList<string> duplicateGroup in _duplicateNameFinder.GetDuplicateGroups(names))
            {
                if (!duplicateGroup.Contains(_nameField.value))
                    continue;

                List<string> existingNames = new List<string>(duplicateGroup);
                existingNames.Remove(_nameField.value);

                return $"Similar asset already exists: {string.Join(", ", existingNames)}";
            }

            return string.Empty;
        }

        private void CreateAsset()
        {
            Type assetType = _creatableTypes[_typeField.index];

            try
            {
                ScriptableObject createdAsset =
                    _mvvmAssetFactory.Create(assetType, _nameField.value, _folderField.value);

                Selection.activeObject = createdAsset;
                EditorGUIUtility.PingObject(createdAsset);

                OnCreated?.Invoke();
                Close();
            }
            catch (DirectoryNotFoundException directoryNotFoundException)
            {
                ShowCreationError(directoryNotFoundException);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                ShowCreationError(invalidOperationException);
            }
        }

        private void ShowCreationError(Exception exception)
        {
            _warningLabel.style.color = Color.red;
            _warningLabel.text = exception.Message;
        }

        private string GetSelectedFolderPath()
        {
            if (ReferenceEquals(Selection.activeObject, null))
                return DefaultFolderPath;

            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(selectedPath))
                return DefaultFolderPath;

            if (AssetDatabase.IsValidFolder(selectedPath))
                return selectedPath;

            return Path.GetDirectoryName(selectedPath).Replace("\\", "/");
        }
    }
}
