// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Provider
{
    internal class QbeResxQueryProvider(Native.Provider.IQueryProvider decorable) : Native.Provider.IQueryProvider
    {
        public Task<string> RetrieveAsync(string name)
        {
            string? str = Queries.ResourceManager.GetString(name);
            if (!string.IsNullOrWhiteSpace(str))
            {
                return Task.FromResult(str);
            }
            else
            {
                return decorable.RetrieveAsync(name);
            }
        }
    }
}
