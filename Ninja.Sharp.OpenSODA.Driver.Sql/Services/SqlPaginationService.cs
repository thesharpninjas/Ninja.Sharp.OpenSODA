// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Models;
using Ninja.Sharp.OpenSODA.Models;

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Native.Services
{
    internal interface IPaginationService
    {
        void ValidatePagination(Page? pagination);
        SqlPaginationData GetPaginationAndFilterStringQuery(Page? pagination);
    }

    internal class SqlPaginationService : IPaginationService
    {
        public SqlPaginationData GetPaginationAndFilterStringQuery(Page? pagination)
        {
            SqlPaginationData data = new();
            if (pagination != null)
            {
                data.Skip = (pagination.PageNumber - 1) * pagination.ItemsPerPage;
                data.Limit = pagination.ItemsPerPage;
            }
            return data;
        }

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
    }
}
