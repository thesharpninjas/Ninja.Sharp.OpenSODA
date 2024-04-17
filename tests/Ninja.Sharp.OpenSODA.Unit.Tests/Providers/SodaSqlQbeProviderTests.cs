using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Models;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Provider;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Models;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Provider;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Services;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Interfaces;
using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Queries;
using Ninja.Sharp.OpenSODA.Queries.Operations;
using Ninja.Sharp.OpenSODA.Queries.Primitives;
using System.Data;
using static Ninja.Sharp.OpenSODA.Unit.Tests.AppFixture;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Providers
{
    public class SodaSqlQbeProviderTests(AppFixture fixture) : IClassFixture<AppFixture>
    {
        private readonly AppFixture fixture = fixture;

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData]
        public async Task Filter_OK(Page? sodaPagination)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new(MockBehavior.Strict);
            mockQueryProvider.Setup(p => p.RetrieveAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            mockPaginationService.Setup(p => p.GetPaginationAndFilterStringQuery(It.IsAny<Page?>(), It.IsAny<SodaQbe>())).Returns(sodaPagination == null ? new SqlPaginationData() : fixture.Fixture.Create<SqlPaginationData>());
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.Setup(r => r.Dispose());
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n != 5))).Returns(fixture.Fixture.Create<string>());
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n != "JSON_DOCUMENT"))).Returns(0);
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.Setup(r => r.GetFieldType(It.IsAny<int>())).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n != 5))).Returns(fixture.Fixture.Create<string>());
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            mockDataReader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);
            mockDataReader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns(fixture.Fixture.Create<DateTime>());
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            ICollection<Item<TestClass>> result = await myProvider.FilterAsync<TestClass>("{ \"name\": \"abc\" }", sodaPagination);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task Filter_Query_OK()
        {
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new(MockBehavior.Strict);
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            // Arrange
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            mockPaginationService.Setup(p => p.GetPaginationAndFilterStringQuery(It.IsAny<Page?>(), It.IsAny<SodaQbe>())).Returns(fixture.Fixture.Create<SqlPaginationData>());
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.Setup(r => r.Dispose());
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n != 5))).Returns(fixture.Fixture.Create<string>());
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n != "JSON_DOCUMENT"))).Returns(0);
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.Setup(r => r.GetFieldType(It.IsAny<int>())).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n != 5))).Returns(fixture.Fixture.Create<string>());
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            mockDataReader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);
            mockDataReader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns(fixture.Fixture.Create<DateTime>());
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());
            Query query = new();
            query.With(new Or()
                .With(new OString("nome", "abc"))
                .With(new OString("nome", "ddd"))
                );

            // Act
            ICollection<Item<TestClass>> result = await myProvider.FilterAsync<TestClass>(query);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task Filter_WrongQuery_InvalidData()
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new(MockBehavior.Strict);
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act + Assert
            await Assert.ThrowsAsync<Exceptions.InvalidDataException>(() => myProvider.FilterAsync<TestClass>(" \"name\": \"abc\" }"));
        }

        [Theory]
        [InlineAutoData]
        public async Task List_Ok(Page? pagination)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new(MockBehavior.Strict);
            mockQueryProvider.Setup(p => p.RetrieveAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);
            Mock<IDataReader> mockDataReader = new(MockBehavior.Strict);
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            mockPaginationService.Setup(p => p.GetPaginationAndFilterStringQuery(It.IsAny<Page?>(), It.IsAny<SodaQbe>())).Returns(fixture.Fixture.Create<SqlPaginationData>());
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.Setup(r => r.Dispose());
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n != 5))).Returns(fixture.Fixture.Create<string>());
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n != "JSON_DOCUMENT"))).Returns(0);
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.Setup(r => r.GetFieldType(It.IsAny<int>())).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n != 5))).Returns(fixture.Fixture.Create<string>());
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            mockDataReader.Setup(r => r.IsDBNull(It.IsAny<int>())).Returns(false);
            mockDataReader.Setup(r => r.GetDateTime(It.IsAny<int>())).Returns(fixture.Fixture.Create<DateTime>());
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            mockDocumentDbProvider.Setup(d => d.ListAsync<TestClass>(It.IsAny<Page?>(), It.IsAny<string?>())).ReturnsAsync(fixture.Fixture.Create<ICollection<Item<TestClass>>>());
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            ICollection<Item<TestClass>> result = await myProvider.ListAsync<TestClass>(pagination);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task CreateCollectionIfNotExists_WithType_Ok()
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            mockDocumentDbProvider.Setup(d => d.CreateCollectionIfNotExistsAsync<TestClass>()).Returns(Task.CompletedTask);
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            Exception? exception = await Record.ExceptionAsync(myProvider.CreateCollectionIfNotExistsAsync<TestClass>);

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData("CollectionName")]
        public async Task CreateCollectionIfNotExists_WithCollectionName_Ok(string collectionName)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            mockDocumentDbProvider.Setup(d => d.CreateCollectionIfNotExistsAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            Exception? exception = await Record.ExceptionAsync(() => myProvider.CreateCollectionIfNotExistsAsync(collectionName));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData]
        public async Task Create_Ok(TestClass item)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            mockDocumentDbProvider.Setup(d => d.CreateAsync(It.IsAny<TestClass>(), It.IsAny<string?>())).ReturnsAsync(fixture.Fixture.Create<Item<TestClass>>());
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            Item<TestClass> result = await myProvider.CreateAsync(item);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Update_Ok(TestClass item, string id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            mockDocumentDbProvider.Setup(d => d.UpdateAsync(It.IsAny<TestClass>(), It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync(fixture.Fixture.Create<Item<TestClass>>());
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpdateAsync(item, id);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Upsert_Ok(TestClass item, string id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            mockDocumentDbProvider.Setup(d => d.UpsertAsync(It.IsAny<TestClass>(), It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync(fixture.Fixture.Create<Item<TestClass>>());
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpsertAsync(item, id);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Retrieve_Ok(string id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            mockDocumentDbProvider.Setup(d => d.RetrieveAsync<TestClass>(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync(fixture.Fixture.Create<Item<TestClass>>());
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            Item<TestClass> result = await myProvider.RetrieveAsync<TestClass>(id);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Delete_Ok(string id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDocumentDbProvider> mockDocumentDbProvider = new(MockBehavior.Strict);
            mockDocumentDbProvider.Setup(d => d.DeleteAsync<TestClass>(It.IsAny<string>(), It.IsAny<string?>())).Returns(Task.CompletedTask);
            SqlQbeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, mockDocumentDbProvider.Object, fixture.Logger<SqlQbeProvider>());

            // Act
            Exception? exception = await Record.ExceptionAsync(() => myProvider.DeleteAsync<TestClass>(id));

            // Assert
            Assert.Null(exception);
        }

    }
}
