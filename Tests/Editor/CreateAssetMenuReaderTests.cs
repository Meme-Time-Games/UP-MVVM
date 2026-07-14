using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class CreateAssetMenuReaderTests
    {
        [Test]
        public void GetMenuNameWithTypeName_WhenTypeIsUnknown_ReturnsEmpty()
        {
            CreateAssetMenuReader createAssetMenuReader = new CreateAssetMenuReader();

            string menuName = createAssetMenuReader.GetMenuNameWithTypeName("NoSuchTypeAnywhereSO");

            Assert.IsEmpty(menuName);
        }

        [Test]
        public void GetMenuNameWithTypeName_WhenTypeNameIsNull_ReturnsEmpty()
        {
            CreateAssetMenuReader createAssetMenuReader = new CreateAssetMenuReader();

            string menuName = createAssetMenuReader.GetMenuNameWithTypeName(null);

            Assert.IsEmpty(menuName);
        }

        [Test]
        public void GetMenuNameWithTypeName_WhenTypeIsIntReactiveVariable_ReturnsItsMenuName()
        {
            CreateAssetMenuReader createAssetMenuReader = new CreateAssetMenuReader();

            string menuName = createAssetMenuReader.GetMenuNameWithTypeName("IntReactiveVariableSO");

            Assert.AreEqual("ScriptableObjects/MVVM/ReactiveVariables/Int", menuName);
        }

        [Test]
        public void GetMenuNameWithTypeName_WhenTypeIsEventViewModel_ReturnsItsMenuName()
        {
            CreateAssetMenuReader createAssetMenuReader = new CreateAssetMenuReader();

            string menuName = createAssetMenuReader.GetMenuNameWithTypeName("EventViewModelSO");

            Assert.AreEqual("ScriptableObjects/MVVM/EventViewModelSO", menuName);
        }
    }
}
