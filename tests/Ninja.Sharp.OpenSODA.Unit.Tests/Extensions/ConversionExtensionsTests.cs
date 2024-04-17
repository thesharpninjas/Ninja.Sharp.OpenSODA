using AutoFixture.Xunit2;
using Ninja.Sharp.OpenSODA.Extensions;
using static Ninja.Sharp.OpenSODA.Unit.Tests.AppFixture;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Extensions
{
    public class ConversionExtensionsTests
    {
        [Theory]
        [InlineAutoData]
        public void Serialize_Ok(TestClass testClass)
        {
            string serialized = testClass.Serialize();

            Assert.NotNull(serialized);
            Assert.NotEmpty(serialized);
        }

        [Theory]
        [InlineAutoData]
        public void Deserialize_Ok(TestClass testClass)
        {
            string serialized = testClass.Serialize();
            TestClass deserialized = serialized.Deserialize<TestClass>();

            Assert.NotNull(deserialized);
            Assert.NotEmpty(serialized);
        }

        [Theory]
        [InlineAutoData]
        public void Deserialize_Ko(TestClass testClass)
        {
            string serialized = testClass.Serialize();
            serialized = serialized[..^5];

            Assert.Throws<ArgumentException>(() => serialized.Deserialize<TestClass>());
        }

        [Theory]
        [InlineAutoData("")]
        [InlineAutoData("null")]
        public void Deserialize_NullOrEmpty_Ko(string serialized)
        {
            Assert.Throws<ArgumentException>(() => serialized.Deserialize<TestClass>());
        }
    }
}
