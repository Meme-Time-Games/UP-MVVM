using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class ReferenceIndexBuilder
    {
        private const int AssetsPerTick = 50;
        private static readonly string[] ScannedExtensions = { ".prefab", ".unity", ".asset" };

        private readonly YamlGuidExtractor _yamlGuidExtractor = new YamlGuidExtractor();
        private readonly ReferenceIndexCache _referenceIndexCache = new ReferenceIndexCache();

        private ReferenceIndex _referenceIndex;
        private List<string> _pendingPaths;
        private int _nextPathIndex;
        private Action<ReferenceIndex> _onBuilt;

        public void BuildAsync(Action<ReferenceIndex> onBuilt)
        {
            _onBuilt = onBuilt;
            _referenceIndex = new ReferenceIndex(new Dictionary<string, List<string>>());
            _pendingPaths = GetScannablePaths();
            _nextPathIndex = 0;

            EditorApplication.update -= BuildNextChunk;
            EditorApplication.update += BuildNextChunk;
        }

        public void SetIndexForPath(string assetPath, ReferenceIndex referenceIndex)
        {
            referenceIndex.RemoveReferencingPath(assetPath);

            if (!IsScannable(assetPath))
                return;

            foreach (string guid in GetGuidsInFileWithPath(assetPath))
                referenceIndex.AddReferenceWithGuid(guid, assetPath);
        }

        private List<string> GetScannablePaths()
        {
            return AssetDatabase.GetAllAssetPaths().Where(IsScannable).ToList();
        }

        private bool IsScannable(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal) &&
                !assetPath.StartsWith("Packages/", StringComparison.Ordinal))
                return false;

            return ScannedExtensions.Contains(Path.GetExtension(assetPath));
        }

        private IReadOnlyCollection<string> GetGuidsInFileWithPath(string assetPath)
        {
            if (!File.Exists(assetPath))
                return Array.Empty<string>();

            try
            {
                return _yamlGuidExtractor.GetGuidsFromYaml(File.ReadAllText(assetPath));
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"MVVM Explorer could not read {assetPath}: {exception.Message}");
                return Array.Empty<string>();
            }
        }

        private void BuildNextChunk()
        {
            int lastPathIndex = Math.Min(_nextPathIndex + AssetsPerTick, _pendingPaths.Count);

            for (int pathIndex = _nextPathIndex; pathIndex < lastPathIndex; pathIndex++)
                IndexPath(_pendingPaths[pathIndex]);

            _nextPathIndex = lastPathIndex;

            if (IsCancelledByUser())
                return;

            if (_nextPathIndex < _pendingPaths.Count)
                return;

            FinishBuild();
        }

        private void IndexPath(string assetPath)
        {
            foreach (string guid in GetGuidsInFileWithPath(assetPath))
                _referenceIndex.AddReferenceWithGuid(guid, assetPath);
        }

        private bool IsCancelledByUser()
        {
            if (_pendingPaths.Count == 0)
                return false;

            float progress = (float)_nextPathIndex / _pendingPaths.Count;

            if (!EditorUtility.DisplayCancelableProgressBar(
                    "MVVM Explorer", $"Indexing references ({_nextPathIndex}/{_pendingPaths.Count})", progress))
                return false;

            StopBuild();
            _onBuilt?.Invoke(_referenceIndex);

            return true;
        }

        private void FinishBuild()
        {
            StopBuild();
            _referenceIndexCache.Save(_referenceIndex);
            _onBuilt?.Invoke(_referenceIndex);
        }

        private void StopBuild()
        {
            EditorApplication.update -= BuildNextChunk;
            EditorUtility.ClearProgressBar();
        }
    }
}
