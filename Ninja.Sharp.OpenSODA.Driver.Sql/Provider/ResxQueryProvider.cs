// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Native.Provider
{
    internal interface IQueryProvider
    {
        public Task<string> RetrieveAsync(string name);
    }

    internal class ResxQueryProvider : IQueryProvider
    {
        public Task<string> RetrieveAsync(string name)
        {
            string? str = Queries.ResourceManager.GetString(name);
            if (!string.IsNullOrWhiteSpace(str))
            {
                return Task.FromResult(str);
            }
            throw new ArgumentException($"[SODA] Query {name} was not found.");
        }
    }
}
