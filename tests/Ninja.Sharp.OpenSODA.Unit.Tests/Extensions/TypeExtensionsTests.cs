using AutoFixture.Xunit2;
using Ninja.Sharp.OpenSODA.Exceptions;
using Ninja.Sharp.OpenSODA.Extensions;
using static Ninja.Sharp.OpenSODA.Unit.Tests.AppFixture;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Extensions
{
    public class TypeExtensionsTests
    {
        public TypeExtensionsTests()
        {
        }

        [Fact]
        public void GetCollectionName_Attribute_Ok()
        {
            string collectionName = typeof(TestClass).CollectionName();

            Assert.NotNull(collectionName);
            Assert.NotEmpty(collectionName);
        }

        [Fact]
        public void GetCollectionName_Attribute_Empty_Ok()
        {
            string collectionName = typeof(TestClassEmptyName).CollectionName();

            Assert.NotNull(collectionName);
            Assert.NotEmpty(collectionName);
        }

        [Fact]
        public void GetCollectionName_Attribute_NotAlpha_Ok()
        {
            string collectionName = typeof(TestClassNotAlphaName).CollectionName();

            Assert.NotNull(collectionName);
            Assert.NotEmpty(collectionName);
        }

        [Fact]
        public void GetCollectionName_Attribute_NoAttribute_Ok()
        {
            string collectionName = typeof(ITestClassNoAttribute).CollectionName();

            Assert.NotNull(collectionName);
            Assert.NotEmpty(collectionName);
        }

        [Theory]
        [InlineAutoData("NameWithoutUnderscores")]
        [InlineAutoData("Name_With_Underscores")]
        public void GetCollectionName_Parameter_Ok(string name)
        {
            string collectionName = typeof(ITestClassNoAttribute).CollectionName(name);

            Assert.NotNull(collectionName);
            Assert.NotEmpty(collectionName);
        }

        [Theory]
        [InlineAutoData("Name with spaces")]
        [InlineAutoData("?Name-with/other\\characters")]
        public void GetCollectionName_ParameterWithOtherCharacters_SodaConfigurationException(string name)
        {
            Assert.Throws<SodaConfigurationException>(() => typeof(ITestClassNoAttribute).CollectionName(name));
        }
    }
}
