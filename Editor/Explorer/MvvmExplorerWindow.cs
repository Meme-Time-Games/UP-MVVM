using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MVVM.CoreEditor
{
    public class MvvmExplorerWindow : EditorWindow
    {
        private readonly MvvmAssetRepository _mvvmAssetRepository = new MvvmAssetRepository();

        private AssetListPanel _assetListPanel;
        private ReferencesPanel _referencesPanel;
        private RuntimePanel _runtimePanel;
        private ReferenceIndex _referenceIndex;
        private string _pendingSelectionGuid;

        [MenuItem("Tools/MVVM/Explorer")]
        public static void ShowWindow()
        {
            GetWindow<MvvmExplorerWindow>("MVVM Explorer");
        }

        public static void ShowWindowWithAsset(Object asset)
        {
            MvvmExplorerWindow window = GetWindow<MvvmExplorerWindow>("MVVM Explorer");
            window.SelectAsset(asset);
        }

        public void SelectAsset(Object asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            _pendingSelectionGuid = AssetDatabase.AssetPathToGUID(assetPath);

            _assetListPanel?.SelectAssetWithGuid(_pendingSelectionGuid);
        }

        private void CreateGUI()
        {
            _runtimePanel?.Dispose();

            Toolbar toolbar = new Toolbar();

            ToolbarButton createButton = new ToolbarButton(ShowCreatePopup);
            createButton.text = "+ Create";
            toolbar.Add(createButton);

            ToolbarButton rebuildButton = new ToolbarButton(RebuildIndex);
            rebuildButton.text = "Rebuild Index";
            toolbar.Add(rebuildButton);

            _assetListPanel = new AssetListPanel();
            _referencesPanel = new ReferencesPanel();
            _runtimePanel = new RuntimePanel();
            _assetListPanel.OnAssetSelected = ShowAssetDetail;

            TwoPaneSplitView detailSplitView =
                new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Vertical);
            detailSplitView.Add(_referencesPanel.Root);
            detailSplitView.Add(_runtimePanel.Root);

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 280, TwoPaneSplitViewOrientation.Horizontal);
            splitView.Add(_assetListPanel.Root);
            splitView.Add(detailSplitView);

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(splitView);

            LoadAssets();
        }

        private void ShowCreatePopup()
        {
            CreateMvvmAssetPopup.ShowPopup(LoadAssets);
        }

        private void OnDisable()
        {
            _runtimePanel?.Dispose();
        }

        private void ShowAssetDetail(MvvmAsset asset)
        {
            _referencesPanel.ShowAsset(asset, _referenceIndex);
            _runtimePanel.ShowAsset(asset);
        }

        private void RebuildIndex()
        {
            ReferenceIndexProvider.Rebuild(referenceIndex => ShowAssetsWithIndex(referenceIndex));
        }

        private void LoadAssets()
        {
            ReferenceIndexProvider.GetIndex(referenceIndex => ShowAssetsWithIndex(referenceIndex));
        }

        private void ShowAssetsWithIndex(ReferenceIndex referenceIndex)
        {
            if (this == null)
                return;

            _referenceIndex = referenceIndex;
            _assetListPanel.SetAssets(_mvvmAssetRepository.GetAllAssets(), referenceIndex);

            if (string.IsNullOrEmpty(_pendingSelectionGuid))
                return;

            _assetListPanel.SelectAssetWithGuid(_pendingSelectionGuid);
        }
    }
}
