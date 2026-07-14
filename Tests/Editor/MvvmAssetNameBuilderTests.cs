using System;
using NUnit.Framework;

namespace MVVM.CoreEditor.Tests
{
    public class MvvmAssetNameBuilderTests
    {
        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameIsGiven_AppendsTheTypeName()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            string assetName = builder.GetAssetNameWithTypeNameAndName("IntReactiveVariableSO", "PlayerHealth");

            Assert.AreEqual("PlayerHealthIntReactiveVariableSO", assetName);
        }

        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameAlreadyEndsWithTheTypeName_DoesNotAppendItTwice()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            string assetName = builder.GetAssetNameWithTypeNameAndName(
                "IntReactiveVariableSO", "PlayerHealthIntReactiveVariableSO");

            Assert.AreEqual("PlayerHealthIntReactiveVariableSO", assetName);
        }

        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameHasSurroundingWhitespace_TrimsIt()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            string assetName = builder.GetAssetNameWithTypeNameAndName("EventViewModelSO", "  OnPlayerDied  ");

            Assert.AreEqual("OnPlayerDiedEventViewModelSO", assetName);
        }

        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameIsWhitespace_ThrowsArgumentException()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            Assert.Throws<ArgumentException>(
                () => builder.GetAssetNameWithTypeNameAndName("IntReactiveVariableSO", "   "));
        }

        [Test]
        public void GetAssetNameWithTypeNameAndName_WhenNameIsNull_ThrowsArgumentException()
        {
            MvvmAssetNameBuilder builder = new MvvmAssetNameBuilder();

            Assert.Throws<ArgumentException>(
                () => builder.GetAssetNameWithTypeNameAndName("IntReactiveVariableSO", null));
        }
    }
}
