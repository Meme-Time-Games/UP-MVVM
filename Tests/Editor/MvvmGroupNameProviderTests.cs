using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class MvvmGroupNameProviderTests
    {
        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuHasFeatureBeforeReactiveVariables_ReturnsTheFeature()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                "ScriptableObjects/MVVM/ReactiveVariables/Bool", "Assets/Whatever/A.asset");

            Assert.AreEqual("MVVM", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuHasFeatureAfterReactiveVariable_ReturnsTheFeature()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                "ScriptableObjects/ReactiveVariable/Fishing/FishData", "Assets/Whatever/A.asset");

            Assert.AreEqual("Fishing", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuHasNoFeatureSegment_FallsBackToTheFolder()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                "ScriptableObjects/ReactiveVariable/FriendData", "Assets/Friends/Data/FriendData.asset");

            Assert.AreEqual("Friends", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuNameIsEmpty_FallsBackToTheFolder()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                string.Empty, "Assets/Chat/Events/OnMessageReceived.asset");

            Assert.AreEqual("Chat", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenFolderIsAllGenericNames_ReturnsUngrouped()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                string.Empty, "Assets/Resources/OnThing.asset");

            Assert.AreEqual("Ungrouped", groupName);
        }

        [Test]
        public void GetGroupNameWithMenuNameAndPath_WhenMenuIsPokerNestedUnderMinigames_ReturnsTheFirstFeature()
        {
            MvvmGroupNameProvider provider = new MvvmGroupNameProvider();

            string groupName = provider.GetGroupNameWithMenuNameAndPath(
                "ScriptableObjects/Minigames/Poker/ReactiveVariables/PokerRoomData", "Assets/A.asset");

            Assert.AreEqual("Minigames", groupName);
        }
    }
}
