// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Queries.Operations;
using System.Text.Json.Nodes;

namespace Ninja.Sharp.OpenSODA.Queries
{
    public class Query
    {
        private JsonObject _data;
        private string _sql;

        public Query()
        {
            _data = [];
            _sql = string.Empty;
        }

        public Query With(Operation operation)
        {
            _data = operation.GenerateQbeQuery(_data);
            _sql = operation.GenerateSqlNativeQuery();
            return this;
        }

        public string ToQbeString()
        {
            return _data.ToString();
        }

        public string ToSqlNativeString()
        {
            return _sql;
        }
    }
}
