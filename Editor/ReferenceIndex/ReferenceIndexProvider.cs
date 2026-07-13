using System;

namespace MVVM.CoreEditor
{
    public static class ReferenceIndexProvider
    {
        private static ReferenceIndex _referenceIndex;
        private static readonly ReferenceIndexCache Cache = new ReferenceIndexCache();

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

            _referenceIndex = Cache.Load();

            if (HasIndex())
            {
                onReady(_referenceIndex);
                return;
            }

            Rebuild(onReady);
        }

        public static void Rebuild(Action<ReferenceIndex> onReady)
        {
            ReferenceIndexBuilder referenceIndexBuilder = new ReferenceIndexBuilder();

            referenceIndexBuilder.BuildAsync(builtIndex =>
            {
                _referenceIndex = builtIndex;
                onReady(builtIndex);
            });
        }
    }
}
