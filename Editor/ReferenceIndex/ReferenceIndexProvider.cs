using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public static class ReferenceIndexProvider
    {
        private static readonly ReferenceIndexCache _cache = new ReferenceIndexCache();
        private static readonly List<Action<ReferenceIndex>> _pendingCallbacks = new List<Action<ReferenceIndex>>();

        private static ReferenceIndex _referenceIndex;
        private static bool _isBuilding;

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
            _pendingCallbacks.Add(onReady);

            if (_isBuilding)
                return;

            _isBuilding = true;

            ReferenceIndexBuilder referenceIndexBuilder = new ReferenceIndexBuilder();

            try
            {
                referenceIndexBuilder.BuildAsync(FinishBuild);
            }
            catch (Exception exception)
            {
                _isBuilding = false;
                _pendingCallbacks.Clear();
                Debug.LogException(exception);
                throw;
            }
        }

        private static void FinishBuild(ReferenceIndex builtIndex)
        {
            _isBuilding = false;
            _referenceIndex = builtIndex;

            List<Action<ReferenceIndex>> callbacksToInvoke = new List<Action<ReferenceIndex>>(_pendingCallbacks);
            _pendingCallbacks.Clear();

            foreach (Action<ReferenceIndex> pendingCallback in callbacksToInvoke)
                InvokeSafely(pendingCallback, builtIndex);
        }

        private static void InvokeSafely(Action<ReferenceIndex> callback, ReferenceIndex builtIndex)
        {
            try
            {
                callback(builtIndex);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
