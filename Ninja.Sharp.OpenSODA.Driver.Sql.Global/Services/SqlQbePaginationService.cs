// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Models;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Models;
using Ninja.Sharp.OpenSODA.Enums;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Models;
using System.Text;

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Services
{
    internal interface IPaginationService
    {
        void ValidatePagination(Page? pagination);
        SqlPaginationData GetPaginationAndFilterStringQuery(Page? pagination, SodaQbe deserializedQbe);
    }
    internal class SqlQbePaginationService : IPaginationService
    {
        public void ValidatePagination(Page? pagination)
        {
            if (pagination != null)
            {
                if (pagination.PageNumber <= 0)
                {
                    throw new Exceptions.InvalidDataException($"[SODA] Page number is invalid: {pagination.PageNumber}.");
                }
                if (pagination.ItemsPerPage <= 0)
                {
                    throw new Exceptions.InvalidDataException($"[SODA] Items per page value is invalid: {pagination.ItemsPerPage}.");
                }
            }
        }
        private static string GetOrderingQuery(Page pagination)
        {
            return $"\"$orderby\":[{{\"path\":\"{pagination.OrderingPath}\",\"order\":\"{(pagination.Ordering == Ordering.Ascending ? "asc" : "desc")}\"}}]";
        }

        public SqlPaginationData GetPaginationAndFilterStringQuery(Page? pagination, SodaQbe deserializedQbe)
        {
            SqlPaginationData data = new();
            if (pagination != null)
            {
                StringBuilder sb = new();
                if (!string.IsNullOrWhiteSpace(pagination.OrderingPath) && deserializedQbe.OrderBy == null)
                {
                    sb.Append($"{{\"$query\":{deserializedQbe.Query?.Serialize() ?? "{}"},{GetOrderingQuery(pagination)}}}");
                }
                else
                {
                    sb.Append($"{{\"$query\":{deserializedQbe.Query?.Serialize() ?? "{}"},\"$orderby\":{deserializedQbe.OrderBy?.Serialize() ?? "{}"}}}");
                }
                data.FilterString = sb.ToString();
                data.Skip = (pagination.PageNumber - 1) * pagination.ItemsPerPage;
                data.Limit = pagination.ItemsPerPage;
            }
            else
            {
                data.FilterString = $"{{\"$query\":{deserializedQbe.Query?.Serialize() ?? "{}"},\"$orderby\":{deserializedQbe.OrderBy?.Serialize() ?? "{}"}}}";
            }
            return data;
        }
    }
}
