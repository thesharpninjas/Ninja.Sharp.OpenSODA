using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Models;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Provider;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Services;
using Ninja.Sharp.OpenSODA.Exceptions;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Queries;
using Ninja.Sharp.OpenSODA.Queries.Operations;
using Ninja.Sharp.OpenSODA.Queries.Primitives;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text;
using static Ninja.Sharp.OpenSODA.Unit.Tests.AppFixture;
using InvalidDataException = Ninja.Sharp.OpenSODA.Exceptions.InvalidDataException;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Providers
{
    public class SodaSqlNativeProviderTests(AppFixture fixture) : IClassFixture<AppFixture>
    {
        private readonly AppFixture fixture = fixture;

        [Fact]
        public async Task List_OK()
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            mockPaginationService.Setup(p => p.GetPaginationAndFilterStringQuery(It.IsAny<Page?>())).Returns(fixture.Fixture.Create<SqlPaginationData>());
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 0))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 0))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            ICollection<Item<TestClass>> result = await myProvider.ListAsync<TestClass>();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData]
        public async Task Filter_OK(Page? sodaPagination)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            mockPaginationService.Setup(p => p.GetPaginationAndFilterStringQuery(It.IsAny<Page?>())).Returns(sodaPagination == null ? new SqlPaginationData() : fixture.Fixture.Create<SqlPaginationData>());
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 0))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 0))).Returns(fixture.Fixture.Create<TestClass>().Serialize());


            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            ICollection<Item<TestClass>> result = await myProvider.FilterAsync<TestClass>("{ \"name\": \"abc\" }", sodaPagination);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task Filter_Query_OK()
        {
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            // Arrange
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            mockPaginationService.Setup(p => p.GetPaginationAndFilterStringQuery(It.IsAny<Page?>())).Returns(fixture.Fixture.Create<SqlPaginationData>());
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 0))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 0))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());
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
        public async Task CreateCollection_OK()
        {
            // Arrange
            Mock<IDbCommand> mockDbCommand = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDataParameterCollection> mockParameterCollection = new();

            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            List<OracleParameter> parameters = [];
            mockParameterCollection.Setup(p => p.Add(It.IsAny<object?>())).Callback<object?>(p =>
            {
                if (p is OracleParameter oracleParameter)
                {
                    parameters.Add(oracleParameter);
                }
            }).Returns(1);
            mockDbCommand.Setup(c => c.Parameters).Returns(mockParameterCollection.Object);
            mockDbCommand.Setup(c => c.ExecuteNonQuery()).Callback(() =>
            {
                parameters.ForEach(p => p.Value = false);
            }).Returns(1);
            SqlNativeProvider myProvider = new(GetDbConnection(mockDbCommand: mockDbCommand), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Exception exception = await Record.ExceptionAsync(myProvider.CreateCollectionIfNotExistsAsync<TestClass>);

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData]
        public async Task Create_OK(TestClass testClass)
        {
            // Arrange
            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);

            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 5))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Item<TestClass> result = await myProvider.CreateAsync(testClass);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Retrieve_OK(Guid id)
        {
            // Arrange

            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockDataReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(true)
                .Returns(true)
                .Returns(false);
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.SetupSequence(r => r.IsDBNull(It.Is<int>(n => n == 5)))
                .Returns(false)
                .Returns(false)
                .Returns(true);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 5))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Item<TestClass> result = await myProvider.RetrieveAsync<TestClass>(id.ToString("n"));

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Retrieve_Bytes_OK(Guid id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockDataReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(true)
                .Returns(true)
                .Returns(false);
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.SetupSequence(r => r.IsDBNull(It.Is<int>(n => n == 5)))
                .Returns(false)
                .Returns(false)
                .Returns(true);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 5))).Returns(typeof(byte[]));
            mockDataReader.Setup(r => r.GetValue(It.Is<int>(n => n == 5))).Returns(Encoding.Default.GetBytes(fixture.Fixture.Create<TestClass>().Serialize()));
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Item<TestClass> result = await myProvider.RetrieveAsync<TestClass>(id.ToString("n"));

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Retrieve_NotFound(Guid id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockDataReader.Setup(r => r.Read()).Returns(false);
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.RetrieveAsync<TestClass>(id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Retrieve_InvalidData(Guid id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 5))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(string.Empty);
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.RetrieveAsync<TestClass>(id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<InvalidDataException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Delete_OK(Guid id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDbCommand> mockDbCommand = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDataParameterCollection> mockParameterCollection = new();
            List<OracleParameter> parameters = [];
            mockParameterCollection.Setup(p => p.Add(It.IsAny<object?>())).Callback<object?>(p =>
            {
                if (p is OracleParameter oracleParameter)
                {
                    parameters.Add(oracleParameter);
                }
            }).Returns(1);
            mockDbCommand.Setup(c => c.Parameters).Returns(mockParameterCollection.Object);
            mockDbCommand.Setup(c => c.ExecuteNonQuery()).Callback(() =>
            {
                parameters.ForEach(p => p.Value = true);
            }).Returns(1);
            SqlNativeProvider myProvider = new(GetDbConnection(mockDbCommand: mockDbCommand), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Exception exception = await Record.ExceptionAsync(() => myProvider.DeleteAsync<TestClass>(id.ToString("n")));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData]
        public async Task Delete_NotFound(Guid id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDbCommand> mockDbCommand = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            Mock<IDataParameterCollection> mockParameterCollection = new();
            List<OracleParameter> parameters = [];
            mockParameterCollection.Setup(p => p.Add(It.IsAny<object?>())).Callback<object?>(p =>
            {
                if (p is OracleParameter oracleParameter)
                {
                    parameters.Add(oracleParameter);
                }
            }).Returns(1);
            mockDbCommand.Setup(c => c.Parameters).Returns(mockParameterCollection.Object);
            mockDbCommand.Setup(c => c.ExecuteNonQuery()).Callback(() =>
            {
                parameters.ForEach(p => p.Value = false);
            }).Returns(1);
            SqlNativeProvider myProvider = new(GetDbConnection(mockDbCommand: mockDbCommand), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Task resultTask = myProvider.DeleteAsync<TestClass>(id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Update_Ok(TestClass testClass, Guid id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 5))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpdateAsync(testClass, id.ToString("n"));

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Update_NotFound(TestClass testClass, Guid id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockDataReader.SetupSequence(r => r.Read()).Returns(false);
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Task resultTask = myProvider.UpdateAsync(testClass, id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await resultTask);
        }


        [Theory]
        [InlineAutoData]
        public async Task Upsert_Ok(TestClass testClass, Guid id)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 5))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpsertAsync(testClass, id.ToString("n"));

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData(null)]
        [InlineAutoData("")]
        [InlineAutoData("@?=")]
        public async Task Upsert_EmptyId_Ok(string? id, TestClass testClass)
        {
            // Arrange
            Mock<Driver.Sql.Native.Provider.IQueryProvider> mockQueryProvider = new();
            mockQueryProvider
                .Setup(p => p.RetrieveAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            Mock<IDataReader> mockDataReader = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockDataReader.SetupSequence(r => r.Read()).Returns(true).Returns(true).Returns(false);
            mockDataReader.SetupGet(r => r.FieldCount).Returns(6);
            mockDataReader.Setup(r => r.GetName(It.Is<int>(n => n == 5))).Returns("JSON_DOCUMENT");
            mockDataReader.Setup(r => r.GetOrdinal(It.Is<string>(n => n == "JSON_DOCUMENT"))).Returns(5);
            mockDataReader.Setup(r => r.GetFieldType(It.Is<int>(n => n == 5))).Returns(typeof(string));
            mockDataReader.Setup(r => r.GetString(It.Is<int>(n => n == 5))).Returns(fixture.Fixture.Create<TestClass>().Serialize());
            SqlNativeProvider myProvider = new(GetDbConnection(mockDataReader), mockQueryProvider.Object, mockPaginationService.Object, fixture.Logger<SqlNativeProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpsertAsync(testClass, id);

            // Assert
            Assert.NotNull(result);
        }
    }
}
