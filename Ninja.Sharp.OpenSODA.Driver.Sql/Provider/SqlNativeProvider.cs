// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Exceptions;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Interfaces;
using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Queries;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Models;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Extensions;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Services;
using Microsoft.Extensions.Logging;

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Native.Provider
{
    internal class SqlNativeProvider(IDbConnection dbConnection, IQueryProvider queryProvider, IPaginationService paginationService, ILogger<SqlNativeProvider> logger) : IDocumentDbProvider
    {
        private readonly IQueryProvider queryProvider = queryProvider;

        public async Task<ICollection<Item<T>>> ListAsync<T>(Page? pagination = null, string? collection = null) where T : class, new()
        {
            return await FilterAsync<T>(string.Empty, pagination, collection);
        }

        public Task CreateCollectionIfNotExistsAsync<T>() where T : class, new()
        {
            string collectionName = typeof(T).CollectionName();
            return CreateCollectionIfNotExistsAsync(collectionName);
        }

        public async Task CreateCollectionIfNotExistsAsync(string collection)
        {
            string checkCollectionSql = await queryProvider.RetrieveAsync("checkcollection");
            string createCollectionSql = await queryProvider.RetrieveAsync("createcollection");
            try
            {
                dbConnection.Open();

                using IDbCommand getCommand = dbConnection.CreateCommand();
                getCommand.Parameters.Add(new OracleParameter("name", OracleDbType.NVarchar2) { Value = collection });
                OracleParameter exists = new("collectionexists", OracleDbType.Boolean, ParameterDirection.Output) { DbType = DbType.Boolean };
                getCommand.Parameters.Add(exists);
                getCommand.CommandText = checkCollectionSql;

                logger.LogDebug("[SODA][CreateCollectionIfNotExists] Executing query: {Query} with name {Name}.", checkCollectionSql, collection);
                getCommand.ExecuteNonQuery();

                logger.LogInformation("[SODA] Existence check of collection {CollectionName} - Result: {Result}.", collection, (bool)exists.Value);

                if (!(bool)exists.Value)
                {
                    using IDbCommand createCommand = dbConnection.CreateCommand();
                    createCommand.Parameters.Add(new OracleParameter("name", OracleDbType.NVarchar2) { Value = collection });
                    createCommand.CommandText = createCollectionSql;
                    logger.LogDebug("[SODA][CreateCollectionIfNotExists] Executing query: {Query} with name {Name}.", createCollectionSql, collection);
                    createCommand.ExecuteNonQuery();

                    logger.LogInformation("[SODA] Created collection {CollectionName}.", collection);
                }
                await Task.CompletedTask;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public async Task<Item<T>> CreateAsync<T>(T item, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            string document = item.Serialize();

            byte[] documentBytes = Encoding.UTF8.GetBytes(document);

            string commandText = await queryProvider.RetrieveAsync("create");
            commandText = commandText.Replace("[[COLLECTIONNAME]]", myCollection);

            try
            {
                dbConnection.Open();

                using IDbCommand command = dbConnection.CreateCommand();
                command.Parameters.Add(new OracleParameter("name", OracleDbType.NVarchar2) { Value = myCollection });
                command.Parameters.Add(new OracleParameter("document", OracleDbType.Blob) { Value = documentBytes });
                command.Parameters.Add(new OracleParameter("mycursor", OracleDbType.RefCursor, ParameterDirection.Output));
                command.CommandText = commandText;
                logger.LogDebug("[SODA][Create] Executing query: {Query} with name {Name}.", commandText, myCollection);

                using IDataReader reader = command.ExecuteReader();
                var items = reader.ReadItems<T>();

                logger.LogInformation("[SODA] Created item in collection {Collection}. ID: {ItemId}.", myCollection, items.First().Id);

                return items.First();
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public async Task<Item<T>> UpdateAsync<T>(T item, string id, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            string document = item.Serialize();
            string alphaId = Regex.Replace(id, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5));

            byte[] documentBytes = Encoding.UTF8.GetBytes(document);

            string commandText = await queryProvider.RetrieveAsync("update");

            commandText = commandText.Replace("[[COLLECTIONNAME]]", myCollection);

            try
            {
                dbConnection.Open();

                using IDbCommand command = dbConnection.CreateCommand();
                command.Parameters.Add(new OracleParameter("name", OracleDbType.NVarchar2) { Value = myCollection });
                command.Parameters.Add(new OracleParameter("document", OracleDbType.Blob) { Value = documentBytes });
                command.Parameters.Add(new OracleParameter("key", OracleDbType.NVarchar2) { Value = alphaId });
                command.Parameters.Add(new OracleParameter("mycursor", OracleDbType.RefCursor, ParameterDirection.Output));

                command.CommandText = commandText;
                logger.LogDebug("[SODA][Update] Executing query: {Query} with name {Name} and key {Key}.", commandText, myCollection, alphaId);

                using IDataReader reader = command.ExecuteReader();
                ICollection<Item<T>> items = reader.ReadItems<T>();

                if (items.Count == 0)
                {
                    throw new NotFoundException($"[SODA] ID {alphaId} not found for collection {myCollection}.");
                }

                logger.LogInformation("[SODA] Updated item in collection {Collection}. ID: {ItemId}.", myCollection, items.First().Id);

                return items.First();
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public async Task<Item<T>> UpsertAsync<T>(T item, string? id = null, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            string document = item.Serialize();
            string alphaId = Regex.Replace(id ?? string.Empty, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5));

            byte[] documentBytes = Encoding.UTF8.GetBytes(document);

            if (string.IsNullOrWhiteSpace(alphaId))
            {
                return await CreateAsync(item);
            }

            try
            {
                dbConnection.Open();

                string commandText = await queryProvider.RetrieveAsync("upsert");
                commandText = commandText.Replace("[[COLLECTIONNAME]]", myCollection);

                using IDbCommand command = dbConnection.CreateCommand();
                command.Parameters.Add(new OracleParameter("name", OracleDbType.NVarchar2) { Value = myCollection });
                command.Parameters.Add(new OracleParameter("document", OracleDbType.Blob) { Value = documentBytes });
                command.Parameters.Add(new OracleParameter("key", OracleDbType.NVarchar2) { Value = alphaId });
                command.Parameters.Add(new OracleParameter("mycursor", OracleDbType.RefCursor, ParameterDirection.Output));

                command.CommandText = commandText;
                logger.LogDebug("[SODA][Upsert] Executing query: {Query} with name {Name} and key {Key}.", commandText, myCollection, alphaId);

                using IDataReader reader = command.ExecuteReader();
                ICollection<Item<T>> items = reader.ReadItems<T>();

                logger.LogInformation("[SODA] Upserted item in collection {Collection}. ID: {ItemId}.", myCollection, items.First().Id);

                return items.First();
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public async Task<Item<T>> RetrieveAsync<T>(string id, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            string alphaId = Regex.Replace(id, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5));

            string commandText = await queryProvider.RetrieveAsync("retrieve");
            commandText = commandText.Replace("[[COLLECTIONNAME]]", myCollection);

            try
            {
                dbConnection.Open();

                using IDbCommand command = dbConnection.CreateCommand();
                command.Parameters.Add(new OracleParameter("id", OracleDbType.NVarchar2) { Value = alphaId });
                command.Parameters.Add(new OracleParameter("name", OracleDbType.NVarchar2) { Value = myCollection });
                command.Parameters.Add(new OracleParameter("mycursor", OracleDbType.RefCursor, ParameterDirection.Output));
                command.CommandText = commandText;
                logger.LogDebug("[SODA][Retrieve] Executing query: {Query} with name {Name} and id {Id}.", commandText, myCollection, alphaId);

                using IDataReader reader = command.ExecuteReader();
                var items = reader.ReadItems<T>();

                if (items.Count > 0)
                {
                    logger.LogInformation("[SODA] Retrieved item in collection {Collection}. ID: {ItemId}.", myCollection, items.First().Id);
                    return items.First();
                }

                throw new NotFoundException($"[SODA] ID {alphaId} not found for collection {myCollection}.");
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public async Task DeleteAsync<T>(string id, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            string alphaId = Regex.Replace(id, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(5));

            string commandText = await queryProvider.RetrieveAsync("delete");

            try
            {
                dbConnection.Open();

                using IDbCommand command = dbConnection.CreateCommand();
                command.Parameters.Add(new OracleParameter("name", OracleDbType.NVarchar2) { Value = myCollection });
                command.Parameters.Add(new OracleParameter("key", OracleDbType.NVarchar2) { Value = alphaId });
                OracleParameter removed = new("removed", OracleDbType.Boolean, ParameterDirection.Output) { DbType = DbType.Boolean };
                command.Parameters.Add(removed);

                command.CommandText = commandText;
                logger.LogDebug("[SODA][Delete] Executing query: {Query} with name {Name} and key {Key}.", commandText, myCollection, alphaId);

                command.ExecuteNonQuery();

                logger.LogInformation("[SODA] Executed delete query in collection {Collection}. Result: {Result}.", myCollection, (bool)removed.Value);

                if (!(bool)removed.Value)
                {
                    throw new NotFoundException($"[SODA] ID {alphaId} not found for collection {myCollection}.");
                }
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public async Task<ICollection<Item<T>>> FilterAsync<T>(string qbe, Page? pagination = null, string? collection = null) where T : class, new()
        {
            string myCollection = typeof(T).CollectionName(collection);

            SqlPaginationData filterAndPaginationQuery;
            paginationService.ValidatePagination(pagination);
            filterAndPaginationQuery = paginationService.GetPaginationAndFilterStringQuery(pagination);

            using IDbCommand command = dbConnection.CreateCommand();
            command.Parameters.Add(new OracleParameter("mycursor", OracleDbType.RefCursor, ParameterDirection.Output));

            string skipPart = filterAndPaginationQuery.Skip?.ToString() ?? "0";
            string limitPart = filterAndPaginationQuery.Limit?.ToString() ?? int.MaxValue.ToString();

            string paginationLine = string.Empty;
            if (skipPart != "0" && limitPart != int.MaxValue.ToString())
            {
                paginationLine = $"OFFSET {skipPart} ROWS FETCH NEXT {limitPart} ROWS ONLY";
            }

            string where = string.Empty;
            if (!string.IsNullOrWhiteSpace(qbe))
            {
                where = $"WHERE ({qbe})";
            }

            var sql = await queryProvider.RetrieveAsync("filter");
            sql = sql
                .Replace("[[name]]", $"\"{myCollection}\"")
                .Replace("[[pagination]]", paginationLine)
                .Replace("[[where]]", where);

            try
            {
                dbConnection.Open();

                command.CommandText = sql;
                logger.LogDebug("[SODA][Filter] Executing query: {Query} with name {Name}.", sql, myCollection);

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
            string qbe = query.ToSqlNativeString();
            return FilterAsync<T>(qbe, pagination, collection);
        }
    }
}
