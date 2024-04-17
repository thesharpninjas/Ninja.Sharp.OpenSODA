// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Interfaces;
using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Queries;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Extensions;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Models;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Models;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Services;
using Microsoft.Extensions.Logging;


namespace Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Provider
{
    internal class SqlQbeProvider(IDbConnection dbConnection, Native.Provider.IQueryProvider queryProvider, IPaginationService paginationService, IDocumentDbProvider decorable, ILogger<SqlQbeProvider> logger)
        : IDocumentDbProvider
    {
        private readonly IDbConnection dbConnection = dbConnection;
        private readonly IPaginationService paginationService = paginationService;
        private readonly IDocumentDbProvider decorable = decorable;

        public async Task<ICollection<Item<T>>> FilterAsync<T>(string qbe, Page? pagination = null, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            SqlPaginationData filterAndPaginationQuery;
            try
            {
                paginationService.ValidatePagination(pagination);
                SodaQbe deserializedQbe = new();
                if (!string.IsNullOrWhiteSpace(qbe))
                {
                    deserializedQbe = qbe.Deserialize<SodaQbe>();
                    deserializedQbe.Query ??= qbe.Deserialize<object>();
                }
                filterAndPaginationQuery = paginationService.GetPaginationAndFilterStringQuery(pagination, deserializedQbe);
            }
            catch (Exception ex)
            {
                throw new Exceptions.InvalidDataException("[SODA] Provided filter is invalid.", innerException: ex);
            }

            using IDbCommand command = dbConnection.CreateCommand();

            command.Parameters.Add(new OracleParameter("name", OracleDbType.NVarchar2) { Value = myCollection });
            command.Parameters.Add(new OracleParameter("qbe", OracleDbType.Varchar2) { Value = filterAndPaginationQuery.FilterString });
            command.Parameters.Add(new OracleParameter("mycursor", OracleDbType.RefCursor, ParameterDirection.Output));

            string skipPart = filterAndPaginationQuery.Skip == null ? string.Empty : ".skip(" + filterAndPaginationQuery.Skip + ")";
            string limitPart = filterAndPaginationQuery.Limit == null ? string.Empty : ".limit(" + filterAndPaginationQuery.Limit + ")";

            string commandText = await queryProvider.RetrieveAsync("filter");

            command.CommandText = commandText.Replace("[[COLLECTIONNAME]]", myCollection).Replace("[[SKIPPART]]", skipPart).Replace("[[LIMITPART]]", limitPart);

            logger.LogDebug("[SODA][Filter] Executing query: {Query} with name {Name}.", command.CommandText, myCollection);

            try
            {
                dbConnection.Open();

                using IDataReader reader = command.ExecuteReader();

                ICollection<Item<T>> list = reader.ReadItems<T>();

                logger.LogInformation("[SODA] Filter applied to collection {Collection}. Number of items retrieved: {ItemCount}.", myCollection, list.Count);

                return list;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public Task<ICollection<Item<T>>> FilterAsync<T>(Query query, Page? pagination = null, string? collection = null) where T : class, new()
        {
            string qbe = query.ToQbeString();
            return FilterAsync<T>(qbe, pagination, collection);
        }

        #region decorable

        public Task<ICollection<Item<T>>> ListAsync<T>(Page? pagination = null, string? collection = null) where T : class, new()
        {
            return FilterAsync<T>(string.Empty, pagination);
        }

        public Task CreateCollectionIfNotExistsAsync(string collection)
        {
            return decorable.CreateCollectionIfNotExistsAsync(collection);
        }

        public Task CreateCollectionIfNotExistsAsync<T>() where T : class, new()
        {
            return decorable.CreateCollectionIfNotExistsAsync<T>();
        }

        public Task<Item<T>> CreateAsync<T>(T item, string? collection = null) where T : class, new()
        {
            return decorable.CreateAsync(item);
        }

        public Task<Item<T>> UpdateAsync<T>(T item, string id, string? collection = null) where T : class, new()
        {
            return decorable.UpdateAsync(item, id);
        }

        public Task<Item<T>> UpsertAsync<T>(T item, string? id = null, string? collection = null) where T : class, new()
        {
            return decorable.UpsertAsync(item, id);
        }

        public Task<Item<T>> RetrieveAsync<T>(string id, string? collection = null) where T : class, new()
        {
            return decorable.RetrieveAsync<T>(id);
        }

        public Task DeleteAsync<T>(string id, string? collection = null) where T : class, new()
        {
            return decorable.DeleteAsync<T>(id);
        }

        #endregion
    }
}
