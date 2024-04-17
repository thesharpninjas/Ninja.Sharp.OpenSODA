// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Enums;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Models;
using System.Text;

namespace Ninja.Sharp.OpenSODA.Driver.Rest.Services
{
    internal interface IPaginationService
    {
        void ValidatePagination(Page? pagination);
        void AddPagination(Page? pagination, StringBuilder urlBuilder, Qbe deserializedQbe, ref string qbe);
        void AddPagination(Page? pagination, StringBuilder urlBuilder);
    }

    internal class RestPaginationService : IPaginationService
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

        private static void AddPaginationInternal(Page pagination, StringBuilder urlBuilder)
        {
            urlBuilder.Append($"?offset={(pagination.PageNumber - 1) * pagination.ItemsPerPage}");
            urlBuilder.Append($"&limit={pagination.ItemsPerPage}");
        }

        public void AddPagination(Page? pagination, StringBuilder urlBuilder, Qbe deserializedQbe, ref string qbe)
        {
            if (pagination != null)
            {
                AddPaginationInternal(pagination, urlBuilder);
                if (!string.IsNullOrWhiteSpace(pagination.OrderingPath))
                {
                    qbe = $"{{\"$query\":{deserializedQbe.Query.Serialize()},{GetOrderingQuery(pagination)}}}";
                }
            }
        }

        public void AddPagination(Page? pagination, StringBuilder urlBuilder)
        {
            if (pagination != null)
            {
                AddPaginationInternal(pagination, urlBuilder);
                if (!string.IsNullOrWhiteSpace(pagination.OrderingPath))
                {
                    urlBuilder.Append("&q={");
                    urlBuilder.Append(GetOrderingQuery(pagination));
                    urlBuilder.Append('}');
                }
            }
        }
    }
}
