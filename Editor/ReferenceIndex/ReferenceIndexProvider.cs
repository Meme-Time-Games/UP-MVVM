using System;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public static class ReferenceIndexProvider
    {
        private static readonly TimeSpan StaleBuildTimeout = TimeSpan.FromMinutes(5);

        private static readonly ReferenceIndexCache _cache = new ReferenceIndexCache();

        private static ReferenceIndex _referenceIndex;
        private static bool _isBuilding;
        private static DateTime _buildStartedAtUtc;

        public static bool HasIndex()
        {
            return !ReferenceEquals(_referenceIndex, null);
        }

        public static ReferenceIndex GetLoadedIndex()
        {
            if (ReferenceEquals(_referenceIndex, null))
                throw new InvalidOperationException("The reference index is not built yet. Call GetIndex first.");

            return _referenceIndex;
        }

        public static void GetIndex(Action<ReferenceIndex> onReady)
        {
            if (HasIndex())
            {
                onReady(_referenceIndex);
                return;
            }

            _referenceIndex = _cache.Load();

            if (HasIndex())
            {
                onReady(_referenceIndex);
                return;
            }

            Rebuild(onReady);
        }

        public static void Rebuild(Action<ReferenceIndex> onReady)
        {
            if (IsBuildStuck())
                RecoverFromStuckBuild();

            if (_isBuilding)
                return;

            _isBuilding = true;
            _buildStartedAtUtc = DateTime.UtcNow;

            ReferenceIndexBuilder referenceIndexBuilder = new ReferenceIndexBuilder();

            referenceIndexBuilder.BuildAsync(builtIndex =>
            {
                _isBuilding = false;
                _referenceIndex = builtIndex;
                onReady(builtIndex);
            });
        }

        private static bool IsBuildStuck()
        {
            if (!_isBuilding)
                return false;

            return DateTime.UtcNow - _buildStartedAtUtc > StaleBuildTimeout;
        }

        private static void RecoverFromStuckBuild()
        {
            Debug.LogWarning(
                "MVVM Explorer: a previous reference index build did not report completion " +
                "within the expected time and appears to have died silently. Clearing its " +
                "in-flight state so a new build can start.");

            _isBuilding = false;
        }
    }
}
