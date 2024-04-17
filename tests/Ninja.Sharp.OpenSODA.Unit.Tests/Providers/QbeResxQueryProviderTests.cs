using AutoFixture.Xunit2;
using Moq;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Provider;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Providers
{
    public class QbeResxQueryProviderTests
    {
        [Theory]
        [InlineAutoData("filter")]
        [InlineAutoData]
        public async Task Retrieve_Ok(string name, string returnedString)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new(MockBehavior.Strict);
            mockQueryProvider.Setup(q => q.RetrieveAsync(It.IsAny<string>())).ReturnsAsync(returnedString);
            QbeResxQueryProvider myProvider = new(mockQueryProvider.Object);

            // Act
            string result = await myProvider.RetrieveAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

    }
}
