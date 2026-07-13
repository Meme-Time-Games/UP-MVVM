using System;
using System.Collections.Generic;

namespace MVVM.CoreEditor
{
    public class ReferenceIndex
    {
        private readonly Dictionary<string, List<string>> _referencingPathsByGuid;

        public ReferenceIndex(Dictionary<string, List<string>> referencingPathsByGuid)
        {
            if (ReferenceEquals(referencingPathsByGuid, null))
                throw new ArgumentNullException(nameof(referencingPathsByGuid));

            _referencingPathsByGuid = referencingPathsByGuid;
        }

        public IReadOnlyList<string> GetReferencingPathsWithGuid(string guid)
        {
            if (!_referencingPathsByGuid.ContainsKey(guid))
                return Array.Empty<string>();

            return _referencingPathsByGuid[guid];
        }

        public bool HasReferencesWithGuid(string guid)
        {
            return GetReferencingPathsWithGuid(guid).Count > 0;
        }

        public void AddReferenceWithGuid(string guid, string referencingPath)
        {
            if (!_referencingPathsByGuid.ContainsKey(guid))
                _referencingPathsByGuid.Add(guid, new List<string>());

            if (_referencingPathsByGuid[guid].Contains(referencingPath))
                return;

            _referencingPathsByGuid[guid].Add(referencingPath);
        }

        public void RemoveReferencingPath(string referencingPath)
        {
            foreach (KeyValuePair<string, List<string>> referencingPathsForGuid in _referencingPathsByGuid)
                referencingPathsForGuid.Value.Remove(referencingPath);
        }

        public IReadOnlyCollection<string> GetIndexedGuids()
        {
            return _referencingPathsByGuid.Keys;
        }
    }
}
