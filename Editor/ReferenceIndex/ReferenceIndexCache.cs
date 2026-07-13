using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MVVM.CoreEditor
{
    public class ReferenceIndexCache
    {
        private const int FormatVersion = 1;
        private const string CacheFilePath = "Library/MvvmReferenceIndex.json";

        public ReferenceIndex Load()
        {
            if (!File.Exists(CacheFilePath))
                return null;

            ReferenceIndexCacheData cacheData = JsonUtility.FromJson<ReferenceIndexCacheData>(
                File.ReadAllText(CacheFilePath));

            if (ReferenceEquals(cacheData, null))
                return null;

            if (cacheData.formatVersion != FormatVersion)
                return null;

            return new ReferenceIndex(GetMapFromEntries(cacheData.entries));
        }

        public void Save(ReferenceIndex referenceIndex)
        {
            ReferenceIndexCacheData cacheData = new ReferenceIndexCacheData();
            cacheData.formatVersion = FormatVersion;
            cacheData.entries = GetEntriesFromIndex(referenceIndex);

            File.WriteAllText(CacheFilePath, JsonUtility.ToJson(cacheData));
        }

        private Dictionary<string, List<string>> GetMapFromEntries(List<ReferenceIndexCacheEntry> entries)
        {
            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();

            foreach (ReferenceIndexCacheEntry entry in entries)
                map[entry.guid] = entry.referencingPaths;

            return map;
        }

        private List<ReferenceIndexCacheEntry> GetEntriesFromIndex(ReferenceIndex referenceIndex)
        {
            List<ReferenceIndexCacheEntry> entries = new List<ReferenceIndexCacheEntry>();

            foreach (string guid in referenceIndex.GetIndexedGuids())
            {
                IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(guid);

                if (referencingPaths.Count == 0)
                    continue;

                ReferenceIndexCacheEntry entry = new ReferenceIndexCacheEntry();
                entry.guid = guid;
                entry.referencingPaths = new List<string>(referencingPaths);

                entries.Add(entry);
            }

            return entries;
        }
    }

    [Serializable]
    public class ReferenceIndexCacheData
    {
        public int formatVersion;
        public List<ReferenceIndexCacheEntry> entries = new List<ReferenceIndexCacheEntry>();
    }

    [Serializable]
    public class ReferenceIndexCacheEntry
    {
        public string guid;
        public List<string> referencingPaths = new List<string>();
    }
}
