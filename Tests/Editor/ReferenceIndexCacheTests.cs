using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class ReferenceIndexCacheTests
    {
        private const string CacheFilePath = "Library/MvvmReferenceIndex.json";
        private const string PlayerDiedGuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string EnemySpawnedGuid = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        private const string HudPrefabPath = "Assets/Prefabs/Hud.prefab";
        private const string MenuPrefabPath = "Assets/Prefabs/Menu.prefab";

        private byte[] _originalCacheFileBytes;
        private bool _cacheFileExisted;

        [SetUp]
        public void BackUpExistingCacheFile()
        {
            _cacheFileExisted = File.Exists(CacheFilePath);

            if (_cacheFileExisted)
                _originalCacheFileBytes = File.ReadAllBytes(CacheFilePath);
        }

        [TearDown]
        public void RestoreExistingCacheFile()
        {
            if (_cacheFileExisted)
            {
                File.WriteAllBytes(CacheFilePath, _originalCacheFileBytes);
                return;
            }

            if (File.Exists(CacheFilePath))
                File.Delete(CacheFilePath);
        }

        [Test]
        public void Load_WhenCacheFileDoesNotExist_ReturnsNull()
        {
            if (File.Exists(CacheFilePath))
                File.Delete(CacheFilePath);

            ReferenceIndexCache referenceIndexCache = new ReferenceIndexCache();

            ReferenceIndex referenceIndex = referenceIndexCache.Load();

            Assert.IsNull(referenceIndex);
        }

        [Test]
        public void Load_AfterSave_ReturnsTheSameReferencingPaths()
        {
            ReferenceIndexCache referenceIndexCache = new ReferenceIndexCache();
            ReferenceIndex savedIndex = new ReferenceIndex(new Dictionary<string, List<string>>());
            savedIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            referenceIndexCache.Save(savedIndex);
            ReferenceIndex loadedIndex = referenceIndexCache.Load();

            Assert.AreEqual(new[] { HudPrefabPath }, loadedIndex.GetReferencingPathsWithGuid(PlayerDiedGuid));
        }

        [Test]
        public void Save_WhenGuidHasNoReferencingPaths_DoesNotPersistItsEntry()
        {
            ReferenceIndexCache referenceIndexCache = new ReferenceIndexCache();
            ReferenceIndex savedIndex = new ReferenceIndex(new Dictionary<string, List<string>>());
            savedIndex.AddReferenceWithGuid(EnemySpawnedGuid, MenuPrefabPath);
            savedIndex.RemoveReferencingPath(MenuPrefabPath);

            referenceIndexCache.Save(savedIndex);
            ReferenceIndex loadedIndex = referenceIndexCache.Load();

            CollectionAssert.DoesNotContain(loadedIndex.GetIndexedGuids(), EnemySpawnedGuid);
        }
    }
}
