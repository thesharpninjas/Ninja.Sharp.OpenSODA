using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Models;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Models;
using Ninja.Sharp.OpenSODA.Enums;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Queries;
using Ninja.Sharp.OpenSODA.Queries.Operations;
using Ninja.Sharp.OpenSODA.Queries.Primitives;
using System.Text;
using Xunit.Sdk;
using InvalidDataException = Ninja.Sharp.OpenSODA.Exceptions.InvalidDataException;
using RestPaginationService = Ninja.Sharp.OpenSODA.Driver.Rest.Services.RestPaginationService;
using SqlPaginationService = Ninja.Sharp.OpenSODA.Driver.Sql.Native.Services.SqlPaginationService;
using SqlQbePaginationService = Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Services.SqlQbePaginationService;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Services
{
    public class PaginationServiceTests(AppFixture fixture) : IClassFixture<AppFixture>
    {
        private readonly AppFixture fixture = fixture;

        [Theory]
        [InlineAutoData(1)]
        [InlineAutoData(-1)]
        public void SqlValidate_Ko(int multiplier, ushort positiveValue)
        {
            // Arrange
            Page pagination = fixture.Fixture.Create<Page>();
            pagination.PageNumber = -multiplier * (positiveValue + 1);
            pagination.ItemsPerPage = multiplier * (positiveValue + 1);
            SqlPaginationService paginationService = new();

            // Act

            // Assert
            Assert.Throws<InvalidDataException>(() => paginationService.ValidatePagination(pagination));
        }

        [Theory]
        [InlineAutoData]
        public void SqlValidate_Ok(ushort positiveValue)
        {
            // Arrange
            Page pagination = fixture.Fixture.Create<Page>();
            pagination.PageNumber = positiveValue + 1;
            pagination.ItemsPerPage = positiveValue + 1;
            SqlPaginationService paginationService = new();

            // Act
            Exception? exception = Record.Exception(() => paginationService.ValidatePagination(pagination));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData(1)]
        [InlineAutoData(-1)]
        public void RestValidate_Ko(int multiplier, ushort positiveValue)
        {
            // Arrange
            Page pagination = fixture.Fixture.Create<Page>();
            pagination.PageNumber = -multiplier * (positiveValue + 1);
            pagination.ItemsPerPage = multiplier * (positiveValue + 1);
            RestPaginationService paginationService = new();

            // Act

            // Assert
            Assert.Throws<InvalidDataException>(() => paginationService.ValidatePagination(pagination));
        }

        [Theory]
        [InlineAutoData]
        public void RestValidate_Ok(ushort positiveValue)
        {
            // Arrange
            Page pagination = fixture.Fixture.Create<Page>();
            pagination.PageNumber = positiveValue + 1;
            pagination.ItemsPerPage = positiveValue + 1;
            RestPaginationService paginationService = new();

            // Act
            Exception? exception = Record.Exception(() => paginationService.ValidatePagination(pagination));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData(1)]
        [InlineAutoData(-1)]
        public void SqlQbeValidate_Ko(int multiplier, ushort positiveValue)
        {
            // Arrange
            Page pagination = fixture.Fixture.Create<Page>();
            pagination.PageNumber = -multiplier * (positiveValue + 1);
            pagination.ItemsPerPage = multiplier * (positiveValue + 1);
            SqlQbePaginationService paginationService = new();

            // Act

            // Assert
            Assert.Throws<InvalidDataException>(() => paginationService.ValidatePagination(pagination));
        }

        [Theory]
        [InlineAutoData]
        public void SqlQbeValidate_Ok(ushort positiveValue)
        {
            // Arrange
            Page pagination = fixture.Fixture.Create<Page>();
            pagination.PageNumber = positiveValue + 1;
            pagination.ItemsPerPage = positiveValue + 1;
            SqlQbePaginationService paginationService = new();

            // Act
            Exception? exception = Record.Exception(() => paginationService.ValidatePagination(pagination));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData(Ordering.Ascending)]
        [InlineAutoData(Ordering.Descending)]
        public void AddPagination_Ok(Ordering ordering, Page pagination)
        {
            // Arrange
            pagination.Ordering = ordering;
            RestPaginationService paginationService = new();

            // Act
            Exception exception = Record.Exception(() => paginationService.AddPagination(pagination, new StringBuilder()));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData(Ordering.Ascending)]
        [InlineAutoData(Ordering.Descending)]
        public void AddPaginationWithQbe_Ok(Ordering ordering, Page pagination, string qbe)
        {
            // Arrange
            pagination.Ordering = ordering;
            RestPaginationService paginationService = new();
            Qbe sodaQbe = fixture.Fixture.Create<Qbe>();

            // Act
            Exception exception = Record.Exception(() => paginationService.AddPagination(pagination, new StringBuilder(), sodaQbe, ref qbe));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData(null, null)]
        [InlineAutoData(null)]
        [InlineAutoData]
        public void GetPagination_Ok(string? orderingPath, Page? pagination)
        {
            // Arrange
            if (pagination != null)
            {
                pagination.OrderingPath = orderingPath;
            }
            SqlPaginationService paginationService = new();

            // Act
            SqlPaginationData result = paginationService.GetPaginationAndFilterStringQuery(pagination);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData(true, true, Ordering.Ascending)]
        [InlineAutoData(false, true)]
        [InlineAutoData(true, false, Ordering.Descending)]
        [InlineAutoData(false, false)]
        [InlineAutoData(false, false, Ordering.Ascending, "", null)]
        [InlineAutoData(false, true, Ordering.Ascending, "", null)]
        [InlineAutoData(true, false, Ordering.Ascending, "", null)]
        [InlineAutoData(true, true, Ordering.Ascending, "", null)]
        [InlineAutoData(true, false, Ordering.Ascending, "")]
        public void GetPaginationAndFilterStringQuery_Ok(bool nullOrder, bool nullQuery, Ordering ordering, string orderingPath, Page? pagination)
        {
            // Arrange
            SqlQbePaginationService paginationService = new();
            OrderByQbe orderByQbe = fixture.Fixture.Create<OrderByQbe>();
            Query query = new();
            query.With(new And()
                .With(new OInt(nameof(AppFixture.TestClass.Id), 2))
                .With(new OString(nameof(AppFixture.TestClass.Name), "name1"))
                );
            SodaQbe sodaQbe = new()
            {
                OrderBy = orderByQbe.Serialize(),
                Query = query.ToQbeString().Serialize()
            };
            if (nullOrder)
            {
                sodaQbe.OrderBy = null;
            }
            if (nullQuery)
            {
                sodaQbe.Query = null;
            }
            if (pagination != null)
            {
                pagination.OrderingPath = orderingPath;
                pagination.Ordering = ordering;
            }

            // Act
            SqlPaginationData result = paginationService.GetPaginationAndFilterStringQuery(pagination, sodaQbe);

            // Assert
            Assert.NotNull(result);
        }
    }
}
