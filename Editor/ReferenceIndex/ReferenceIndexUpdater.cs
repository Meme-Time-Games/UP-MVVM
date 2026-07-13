using UnityEditor;

namespace MVVM.CoreEditor
{
    public class ReferenceIndexUpdater : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!ReferenceIndexProvider.HasIndex())
                return;

            ReferenceIndex referenceIndex = ReferenceIndexProvider.GetLoadedIndex();
            ReferenceIndexBuilder referenceIndexBuilder = new ReferenceIndexBuilder();

            foreach (string deletedAsset in deletedAssets)
                referenceIndex.RemoveReferencingPath(deletedAsset);

            foreach (string movedFromAssetPath in movedFromAssetPaths)
                referenceIndex.RemoveReferencingPath(movedFromAssetPath);

            foreach (string importedAsset in importedAssets)
                referenceIndexBuilder.SetIndexForPath(importedAsset, referenceIndex);

            foreach (string movedAsset in movedAssets)
                referenceIndexBuilder.SetIndexForPath(movedAsset, referenceIndex);
        }
    }
}
