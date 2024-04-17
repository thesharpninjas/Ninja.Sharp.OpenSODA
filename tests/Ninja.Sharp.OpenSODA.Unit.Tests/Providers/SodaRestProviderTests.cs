using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using Moq.Protected;
using Ninja.Sharp.OpenSODA.Driver.Rest.Provider;
using Ninja.Sharp.OpenSODA.Driver.Rest.Services;
using Ninja.Sharp.OpenSODA.Exceptions;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Queries;
using Ninja.Sharp.OpenSODA.Queries.Operations;
using Ninja.Sharp.OpenSODA.Queries.Primitives;
using System.Net;
using System.Text;
using static Ninja.Sharp.OpenSODA.Unit.Tests.AppFixture;
using InvalidDataException = Ninja.Sharp.OpenSODA.Exceptions.InvalidDataException;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Providers
{
    public class SodaRestProviderTests(AppFixture fixture) : IClassFixture<AppFixture>
    {
        private readonly AppFixture fixture = fixture;

        [Fact]
        public async Task ListAsync_RetrieveData_OK()
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new(MockBehavior.Strict);
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            mockPaginationService.Setup(p => p.AddPagination(It.IsAny<Page?>(), It.IsAny<StringBuilder>()));
            HttpResponseMessage listOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<Result<TestClass>>().Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(listOk);
            SodaRestProvider myProvider = new(fixture.RestConfiguration, 
                GetClientFactory(mockHttpMessageHandler), 
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            ICollection<Item<TestClass>> result = await myProvider.ListAsync<TestClass>();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ListAsync_Retrieve_Error()
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            HttpResponseMessage listKo = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(listKo);
            SodaRestProvider myProvider = new(fixture.RestConfiguration, 
                GetClientFactory(mockHttpMessageHandler), 
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task<ICollection<Item<TestClass>>> resultTask = myProvider.ListAsync<TestClass>();

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Fact]
        public async Task CreateCollectionIfNotExistsAsync_CreateNewCollection_OK()
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage listOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<CollectionsResult>().Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(listOk);
            SodaRestProvider myProvider = new(fixture.RestConfiguration, 
                GetClientFactory(mockHttpMessageHandler), 
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Exception exception = await Record.ExceptionAsync(() => myProvider.CreateCollectionIfNotExistsAsync<TestClass>());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task CreateCollectionIfNotExistsAsync_CreateNewCollection_ManagedInternalError()
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            
            HttpResponseMessage listOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<CollectionsResult>().Serialize())
            };
            
            HttpResponseMessage putKo = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.Method.Method == "GET"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(listOk);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.Method.Method == "PUT"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(putKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration, 
                GetClientFactory(mockHttpMessageHandler), 
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task resultTask = myProvider.CreateCollectionIfNotExistsAsync<TestClass>();

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Fact]
        public async Task CreateCollectionIfNotExistsAsync_CreateNewCollection_NotFound()
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage listKo = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(listKo);
            SodaRestProvider myProvider = new(fixture.RestConfiguration, 
                GetClientFactory(mockHttpMessageHandler), 
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task resultTask = myProvider.CreateCollectionIfNotExistsAsync<TestClass>();

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Create_OK(TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage createOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<Result<TestClass>>().Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration, 
                GetClientFactory(mockHttpMessageHandler), 
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Item<TestClass> result = await myProvider.CreateAsync(testClass);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Create_Error(TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage createKo = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.CreateAsync(testClass);

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Update_OK(Guid id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            Result<TestClass> updateResult = fixture.Fixture.Create<Result<TestClass>>();
            updateResult.Items.First().Id = id.ToString("n");
            HttpResponseMessage updateOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(updateResult.Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpdateAsync(testClass, id.ToString("n"));

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData(".!##")]
        [InlineAutoData("")]
        public async Task Update_InvalidId(string id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());
            
            // Act
            Task<Item<TestClass>> resultTask = myProvider.UpdateAsync(testClass, id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await resultTask);
        }


        [Theory]
        [InlineAutoData]
        public async Task Update_NotFound(Guid id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage updateKo = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateKo);
            
            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                 GetClientFactory(mockHttpMessageHandler),
                 mockPaginationService.Object,
                 fixture.Logger<SodaRestProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.UpdateAsync(testClass, id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await resultTask);
        }


        [Theory]
        [InlineAutoData]
        public async Task Update_InternalError(Guid id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage updateKo = new()
            {
                StatusCode = HttpStatusCode.InternalServerError
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.UpdateAsync(testClass, id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Update_PutError(Guid id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage updateOk = new()
            {
                StatusCode = HttpStatusCode.OK
            };
            HttpResponseMessage updateKo = new()
            {
                StatusCode = HttpStatusCode.InternalServerError
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.Method.Method == "PUT"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateKo);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.Method.Method == "GET"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.UpdateAsync(testClass, id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Update_AfterReadError(Guid id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage updateOk = new()
            {
                StatusCode = HttpStatusCode.OK
            };
            HttpResponseMessage updateKo = new()
            {
                StatusCode = HttpStatusCode.InternalServerError
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.Method.Method == "PUT"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateOk);
            mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.Method.Method == "GET"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateOk)
                .ReturnsAsync(updateKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.UpdateAsync(testClass, id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData(".!##")]
        [InlineAutoData("")]
        [InlineAutoData(null)]
        public async Task Upsert_EmptyId_Create_Ok(string? id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage createOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<Result<TestClass>>().Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpsertAsync(testClass, id);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Upsert_Create_Ok(Guid id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage createOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<Result<TestClass>>().Serialize())
            };
            HttpResponseMessage getNotFound = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.Method.Method == "POST"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(createOk);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(m => m.Method.Method == "GET"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getNotFound);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpsertAsync(testClass, id.ToString("n"));

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Upsert_Update_Ok(TestClass testClass, Guid id)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            Result<TestClass> updateResult = fixture.Fixture.Create<Result<TestClass>>();
            updateResult.Items.First().Id = id.ToString("n");
            HttpResponseMessage updateOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(updateResult.Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(updateOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Item<TestClass> result = await myProvider.UpsertAsync(testClass, id.ToString("n"));

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Upsert_InternalError(Guid id, TestClass testClass)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage getKo = new()
            {
                StatusCode = HttpStatusCode.InternalServerError
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.UpsertAsync(testClass, id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Retrieve_OK(Guid id)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            Result<TestClass> updateResult = fixture.Fixture.Create<Result<TestClass>>();
            updateResult.Items.First().Id = id.ToString("n");
            HttpResponseMessage getOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(updateResult.Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

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
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            Result<TestClass> updateResult = fixture.Fixture.Create<Result<TestClass>>();
            HttpResponseMessage getOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(updateResult.Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.RetrieveAsync<TestClass>(id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await resultTask);
        }


        [Theory]
        [InlineAutoData]
        public async Task Retrieve_InternalError(Guid id)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage getKo = new()
            {
                StatusCode = HttpStatusCode.InternalServerError
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task<Item<TestClass>> resultTask = myProvider.RetrieveAsync<TestClass>(id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Delete_OK(Guid id)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage deleteOk = new()
            {
                StatusCode = HttpStatusCode.OK
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(deleteOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

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
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage deleteKo = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(deleteKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task resultTask = myProvider.DeleteAsync<TestClass>(id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await resultTask);
        }


        [Theory]
        [InlineAutoData]
        public async Task Delete_InternalError(Guid id)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage deleteKo = new()
            {
                StatusCode = HttpStatusCode.InternalServerError
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(deleteKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>());

            // Act
            Task resultTask = myProvider.DeleteAsync<TestClass>(id.ToString("n"));

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Filter_Ok(string fieldName, string fieldValue, Page pagination)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            HttpResponseMessage filterOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<Result<TestClass>>().Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(filterOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>()); 
            
            string qbe = $"{{\"{fieldName}\":\"{fieldValue}\"}}";

            // Act
            ICollection<Item<TestClass>> result = await myProvider.FilterAsync<TestClass>(qbe, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Filter_OrderInFilter_Ok(string fieldName, string fieldValue, string orderField, Page pagination)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            HttpResponseMessage filterOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<Result<TestClass>>().Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(filterOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>()); 
            
            string qbe = $"{{\"$query\":{{\"{fieldName}\":\"{fieldValue}\"}},\"$orderby\":{{\"path\":\"{orderField}\",\"order\":\"asc\"}}}}";

            // Act
            ICollection<Item<TestClass>> result = await myProvider.FilterAsync<TestClass>(qbe, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineAutoData]
        public async Task Filter_Error(string fieldName, string fieldValue, Page pagination)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage filterKo = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(filterKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>()); 
            
            string qbe = $"{{\"{fieldName}\":\"{fieldValue}\"}}";

            // Act
            Task<ICollection<Item<TestClass>>> resultTask = myProvider.FilterAsync<TestClass>(qbe, pagination);

            // Assert
            await Assert.ThrowsAsync<InternalErrorException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Filter_FilterError(Page pagination)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            HttpResponseMessage filterKo = new()
            {
                StatusCode = HttpStatusCode.NotFound
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(filterKo);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>()); 
            
            string qbe = "{\"aaaa\"}";

            // Act
            Task<ICollection<Item<TestClass>>> resultTask = myProvider.FilterAsync<TestClass>(qbe, pagination);

            // Assert
            await Assert.ThrowsAsync<InvalidDataException>(async () => await resultTask);
        }

        [Theory]
        [InlineAutoData]
        public async Task Filter_Query_Ok(string[] fieldNames, string[] fieldValues, Page pagination)
        {
            // Arrange
            Mock<HttpMessageHandler> mockHttpMessageHandler = new();
            Mock<IPaginationService> mockPaginationService = new();
            mockPaginationService.Setup(p => p.ValidatePagination(It.IsAny<Page?>()));
            HttpResponseMessage filterOk = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fixture.Fixture.Create<Result<TestClass>>().Serialize())
            };
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(filterOk);

            SodaRestProvider myProvider = new(fixture.RestConfiguration,
                GetClientFactory(mockHttpMessageHandler),
                mockPaginationService.Object,
                fixture.Logger<SodaRestProvider>()); 
            
            Query query = new();
            Or queryOr = new();
            for (int i = 0; i < fieldNames.Length; i++)
            {
                queryOr.With(new OString(fieldNames[i], fieldValues[i]));
            }
            query.With(queryOr);

            // Act
            ICollection<Item<TestClass>> result = await myProvider.FilterAsync<TestClass>(query, pagination);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
