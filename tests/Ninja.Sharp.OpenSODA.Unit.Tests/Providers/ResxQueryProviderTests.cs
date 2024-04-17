using AutoFixture.Xunit2;
using Moq;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Provider;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Provider;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Providers
{
    public class ResxQueryProviderTests
    {
        [Theory]
        [InlineAutoData("filter")]
        public async Task Retrieve_Ok(string name)
        {
            // Arrange
            ResxQueryProvider myProvider = new();

            // Act
            string result = await myProvider.RetrieveAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Retrieve_NotFound_ArgumentException(string name)
        {
            // Arrange
            ResxQueryProvider myProvider = new();

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentException>(() => myProvider.RetrieveAsync(name));
        }

    }
}
