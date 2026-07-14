using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class MvvmListRowTests
    {
        [Test]
        public void IsGroupHeader_WhenRowIsAGroupHeader_ReturnsTrue()
        {
            MvvmListRow row = MvvmListRow.CreateGroupHeader("MVVM");

            Assert.IsTrue(row.IsGroupHeader());
        }

        [Test]
        public void IsGroupHeader_WhenRowIsAnAsset_ReturnsFalse()
        {
            MvvmAsset asset = new MvvmAsset("guid", "Assets/A.asset", "A", "IntReactiveVariableSO", false);

            MvvmListRow row = MvvmListRow.CreateAssetRow(asset);

            Assert.IsFalse(row.IsGroupHeader());
        }

        [Test]
        public void Asset_WhenRowIsAnAsset_ReturnsTheAsset()
        {
            MvvmAsset asset = new MvvmAsset("guid", "Assets/A.asset", "A", "IntReactiveVariableSO", false);

            MvvmListRow row = MvvmListRow.CreateAssetRow(asset);

            Assert.AreSame(asset, row.Asset);
        }

        [Test]
        public void GroupName_WhenRowIsAGroupHeader_ReturnsTheGroupName()
        {
            MvvmListRow row = MvvmListRow.CreateGroupHeader("Fishing");

            Assert.AreEqual("Fishing", row.GroupName);
        }
    }
}
