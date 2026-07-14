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

        [Test]
        public void GetDuplicateGroups_WhenOnPrefixWouldOnlyMatchViaOldManglingBug_DoesNotGroupThem()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "OnboardingFinished", "BoardingFinishedX" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.IsEmpty(duplicateGroups);
        }

        [Test]
        public void GetDuplicateGroups_WhenOnPrefixIsAGenuineEventPrefix_StillGroupsThem()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied", "OnPlayerDied" };

            IReadOnlyList<IReadOnlyList<string>> duplicateGroups = duplicateNameFinder.GetDuplicateGroups(names);

            Assert.AreEqual(1, duplicateGroups.Count);
        }

        [Test]
        public void GetSimilarNamesWithName_WhenNameMatchesExactly_ReturnsIt()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDied" };

            IReadOnlyList<string> similarNames = duplicateNameFinder.GetSimilarNamesWithName("PlayerDied", names);

            CollectionAssert.Contains(similarNames, "PlayerDied");
        }

        [Test]
        public void GetSimilarNamesWithName_WhenNameDiffersByOnPrefix_ReturnsIt()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "OnPlayerDied" };

            IReadOnlyList<string> similarNames = duplicateNameFinder.GetSimilarNamesWithName("PlayerDied", names);

            CollectionAssert.Contains(similarNames, "OnPlayerDied");
        }

        [Test]
        public void GetSimilarNamesWithName_WhenNameDiffersByOneCharacter_ReturnsIt()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayersDied" };

            IReadOnlyList<string> similarNames = duplicateNameFinder.GetSimilarNamesWithName("PlayerDied", names);

            CollectionAssert.Contains(similarNames, "PlayersDied");
        }

        [Test]
        public void GetSimilarNamesWithName_WhenNameIsUnrelated_DoesNotReturnIt()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "EnemySpawned" };

            IReadOnlyList<string> similarNames = duplicateNameFinder.GetSimilarNamesWithName("PlayerDied", names);

            Assert.IsEmpty(similarNames);
        }

        [Test]
        public void GetSimilarNamesWithName_WhenNameIsASynonym_DoesNotReturnIt()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "PlayerDeath" };

            IReadOnlyList<string> similarNames = duplicateNameFinder.GetSimilarNamesWithName("PlayerDied", names);

            Assert.IsEmpty(similarNames);
        }

        [Test]
        public void GetSimilarNamesWithName_WhenOnPrefixWouldOnlyMatchViaOldManglingBug_DoesNotReturnIt()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "OnboardingFinished" };

            IReadOnlyList<string> similarNames = duplicateNameFinder.GetSimilarNamesWithName("BoardingFinishedX", names);

            Assert.IsEmpty(similarNames);
        }

        [Test]
        public void GetSimilarNamesWithName_WhenOnPrefixIsAGenuineEventPrefix_StillReturnsIt()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string> { "OnPlayerDied" };

            IReadOnlyList<string> similarNames = duplicateNameFinder.GetSimilarNamesWithName("PlayerDied", names);

            CollectionAssert.Contains(similarNames, "OnPlayerDied");
        }

        [Test]
        public void GetSimilarNamesWithName_WhenNamesListIsEmpty_ReturnsEmpty()
        {
            DuplicateNameFinder duplicateNameFinder = new DuplicateNameFinder();
            List<string> names = new List<string>();

            IReadOnlyList<string> similarNames = duplicateNameFinder.GetSimilarNamesWithName("PlayerDied", names);

            Assert.IsEmpty(similarNames);
        }
    }
}
