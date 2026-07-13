using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MVVM.CoreEditor
{
    public class ReferencesPanel
    {
        private readonly VisualElement _root = new VisualElement();
        private readonly OpenSceneReferenceScanner _openSceneReferenceScanner = new OpenSceneReferenceScanner();

        public VisualElement Root => _root;

        public ReferencesPanel()
        {
            _root.style.flexGrow = 1;
        }

        public void ShowAsset(MvvmAsset asset, ReferenceIndex referenceIndex)
        {
            _root.Clear();

            _root.Add(CreateTitle(asset.Name));
            _root.Add(new Label(asset.TypeName));

            AddIndexedReferences(asset, referenceIndex);
            AddOpenSceneReferences(asset);
        }

        private void AddIndexedReferences(MvvmAsset asset, ReferenceIndex referenceIndex)
        {
            IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(asset.Guid);

            _root.Add(CreateTitle($"References ({referencingPaths.Count})"));

            if (referencingPaths.Count == 0)
            {
                _root.Add(new Label("No references found."));
                return;
            }

            foreach (string referencingPath in referencingPaths)
                _root.Add(CreateAssetButton(referencingPath));
        }

        private void AddOpenSceneReferences(MvvmAsset asset)
        {
            Object targetAsset = AssetDatabase.LoadAssetAtPath<Object>(asset.Path);
            IReadOnlyList<Component> referencingComponents =
                _openSceneReferenceScanner.GetReferencingComponentsWithAsset(targetAsset);

            if (referencingComponents.Count == 0)
                return;

            _root.Add(CreateTitle($"In open scenes ({referencingComponents.Count})"));

            foreach (Component referencingComponent in referencingComponents)
                _root.Add(CreateComponentButton(referencingComponent));
        }

        private Button CreateAssetButton(string referencingPath)
        {
            Button button = new Button(() => SelectAssetWithPath(referencingPath));
            button.text = referencingPath;

            return button;
        }

        private Button CreateComponentButton(Component referencingComponent)
        {
            Button button = new Button(() => SelectGameObject(referencingComponent.gameObject));
            button.text = $"{referencingComponent.gameObject.name} ({referencingComponent.GetType().Name})";

            return button;
        }

        private void SelectAssetWithPath(string referencingPath)
        {
            Object referencedAsset = AssetDatabase.LoadMainAssetAtPath(referencingPath);

            Selection.activeObject = referencedAsset;
            EditorGUIUtility.PingObject(referencedAsset);
        }

        private void SelectGameObject(GameObject referencingGameObject)
        {
            Selection.activeGameObject = referencingGameObject;
            EditorGUIUtility.PingObject(referencingGameObject);
        }

        private Label CreateTitle(string title)
        {
            Label titleLabel = new Label(title);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginTop = 8;

            return titleLabel;
        }
    }
}
