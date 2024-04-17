// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Native.Models
{
    internal class SqlPaginationData
    {
        public string FilterString { get; set; } = "{}";
        public int? Skip { get; set; } = null;
        public int? Limit { get; set; } = null;
    }
}
