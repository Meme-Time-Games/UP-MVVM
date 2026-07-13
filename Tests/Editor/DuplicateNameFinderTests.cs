using System.Collections.Generic;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class DuplicateNameFinderTests
    {
        [Test]
        public void GetDuplicateGroups_WhenNamesDifferOnlyByOnPrefix_GroupsThem()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "OnPlayerDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.AreEqual(1, duplicateGroups.Count);
        }

        [Test]
        public void GetDuplicateGroups_WhenNamesDifferByOneCharacter_GroupsThem()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "PlayersDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.AreEqual(2, duplicateGroups[0].Count);
        }

        [Test]
        public void GetDuplicateGroups_WhenNamesAreUnrelated_ReturnsNoGroups()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "EnemySpawned" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.IsEmpty(duplicateGroups);
        }

        [Test]
        public void GetDuplicateGroups_WhenNamesUseDifferentWordsForTheSameIdea_ReturnsNoGroups()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "PlayerDeath" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.IsEmpty(duplicateGroups);
        }

        [Test]
        public void GetDuplicateGroups_WhenOnlyOneNameIsGiven_ReturnsNoGroups()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.IsEmpty(duplicateGroups);
        }

        [Test]
        public void GetDuplicateGroups_WhenThreeNamesMatch_ReturnsThemInOneGroup()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "OnPlayerDied", "PlayersDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.AreEqual(3, duplicateGroups[0].Count);
        }
    }
}
