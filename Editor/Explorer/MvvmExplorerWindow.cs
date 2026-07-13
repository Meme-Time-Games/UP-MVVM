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
            Toolbar toolbar = new Toolbar();

            ToolbarButton rebuildButton = new ToolbarButton(RebuildIndex);
            rebuildButton.text = "Rebuild Index";
            toolbar.Add(rebuildButton);

            _assetListPanel = new AssetListPanel();

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(_assetListPanel.Root);

            LoadAssets();
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

            _assetListPanel.SetAssets(_mvvmAssetRepository.GetAllAssets(), referenceIndex);

            if (string.IsNullOrEmpty(_pendingSelectionGuid))
                return;

            _assetListPanel.SelectAssetWithGuid(_pendingSelectionGuid);
        }
    }
}
