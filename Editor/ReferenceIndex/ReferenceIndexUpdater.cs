using System;
using UnityEditor;
using UnityEngine;

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
                SetIndexForPathSafely(referenceIndexBuilder, importedAsset, referenceIndex);

            foreach (string movedAsset in movedAssets)
                SetIndexForPathSafely(referenceIndexBuilder, movedAsset, referenceIndex);
        }

        private static void SetIndexForPathSafely(
            ReferenceIndexBuilder referenceIndexBuilder,
            string assetPath,
            ReferenceIndex referenceIndex)
        {
            try
            {
                referenceIndexBuilder.SetIndexForPath(assetPath, referenceIndex);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
