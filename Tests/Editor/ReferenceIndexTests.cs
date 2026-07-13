using System;
using System.Collections.Generic;
using MVVM.CoreEditor;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class ReferenceIndexTests
    {
        private const string PlayerDiedGuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private const string EnemySpawnedGuid = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        private const string HudPrefabPath = "Assets/Prefabs/Hud.prefab";
        private const string MenuPrefabPath = "Assets/Prefabs/Menu.prefab";

        [Test]
        public void Constructor_WhenMapIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReferenceIndex(null));
        }

        [Test]
        public void GetReferencingPathsWithGuid_WhenGuidIsUnknown_ReturnsEmptyList()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();

            IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(PlayerDiedGuid);

            Assert.IsEmpty(referencingPaths);
        }

        [Test]
        public void GetReferencingPathsWithGuid_WhenGuidHasOneReference_ReturnsThatPath()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(PlayerDiedGuid);

            Assert.AreEqual(new[] { HudPrefabPath }, referencingPaths);
        }

        [Test]
        public void HasReferencesWithGuid_WhenGuidIsUnknown_ReturnsFalse()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();

            bool hasReferences = referenceIndex.HasReferencesWithGuid(PlayerDiedGuid);

            Assert.IsFalse(hasReferences);
        }

        [Test]
        public void HasReferencesWithGuid_WhenGuidHasOneReference_ReturnsTrue()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            bool hasReferences = referenceIndex.HasReferencesWithGuid(PlayerDiedGuid);

            Assert.IsTrue(hasReferences);
        }

        [Test]
        public void AddReferenceWithGuid_WhenPathIsAlreadyIndexed_DoesNotDuplicateIt()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            IReadOnlyList<string> referencingPaths = referenceIndex.GetReferencingPathsWithGuid(PlayerDiedGuid);

            Assert.AreEqual(1, referencingPaths.Count);
        }

        [Test]
        public void RemoveReferencingPath_WhenPathReferencesSeveralGuids_RemovesItFromEveryGuid()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);
            referenceIndex.AddReferenceWithGuid(EnemySpawnedGuid, HudPrefabPath);
            referenceIndex.AddReferenceWithGuid(EnemySpawnedGuid, MenuPrefabPath);

            referenceIndex.RemoveReferencingPath(HudPrefabPath);

            Assert.AreEqual(new[] { MenuPrefabPath }, referenceIndex.GetReferencingPathsWithGuid(EnemySpawnedGuid));
        }

        [Test]
        public void RemoveReferencingPath_WhenPathIsTheOnlyReference_LeavesGuidWithNoReferences()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);

            referenceIndex.RemoveReferencingPath(HudPrefabPath);

            Assert.IsFalse(referenceIndex.HasReferencesWithGuid(PlayerDiedGuid));
        }

        [Test]
        public void GetIndexedGuids_WhenTwoGuidsAreIndexed_ReturnsBoth()
        {
            ReferenceIndex referenceIndex = CreateEmptyIndex();
            referenceIndex.AddReferenceWithGuid(PlayerDiedGuid, HudPrefabPath);
            referenceIndex.AddReferenceWithGuid(EnemySpawnedGuid, MenuPrefabPath);

            IReadOnlyCollection<string> indexedGuids = referenceIndex.GetIndexedGuids();

            Assert.AreEqual(2, indexedGuids.Count);
        }

        private ReferenceIndex CreateEmptyIndex()
        {
            return new ReferenceIndex(new Dictionary<string, List<string>>());
        }
    }
}
