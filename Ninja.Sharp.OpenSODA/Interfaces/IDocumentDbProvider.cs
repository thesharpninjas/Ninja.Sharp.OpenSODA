// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Queries;

namespace Ninja.Sharp.OpenSODA.Interfaces
{
    public interface IDocumentDbProvider
    {
        Task CreateCollectionIfNotExistsAsync<T>() where T : class, new();
        Task CreateCollectionIfNotExistsAsync(string collection);

        Task<ICollection<Item<T>>> ListAsync<T>(Page? pagination = null, string? collection = null) where T : class, new();
        Task<ICollection<Item<T>>> FilterAsync<T>(string qbe, Page? pagination = null, string? collection = null) where T : class, new();
        Task<ICollection<Item<T>>> FilterAsync<T>(Query query, Page? pagination = null, string? collection = null) where T : class, new();
        
        Task<Item<T>> CreateAsync<T>(T item, string? collection = null) where T : class, new();
        Task<Item<T>> UpdateAsync<T>(T item, string id, string? collection = null) where T : class, new();
        Task<Item<T>> UpsertAsync<T>(T item, string? id = null, string? collection = null) where T : class, new();
        Task<Item<T>> RetrieveAsync<T>(string id, string? collection = null) where T : class, new();
        Task DeleteAsync<T>(string id, string? collection = null) where T : class, new();


    }
}
