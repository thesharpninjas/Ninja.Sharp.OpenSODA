// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Queries.Enums;
using System.Text.Json.Nodes;

namespace Ninja.Sharp.OpenSODA.Queries.Primitives
{
    public class OInt(string key, int value, Compare operationType = Compare.Equals) : SodaPrimitive(key, operationType)
    {
        private readonly int value = value;

        internal override JsonObject GenerateQbeQuery(JsonObject obj)
        {
            JsonObject item = new()
            {
                { Comparison.ToQbeOperator(), value }
            };

            obj.Add(Key, item);
            return obj;
        }

        internal override string GenerateSqlNativeQuery()
        {
            return $"json_value(\"JSON_DOCUMENT\", '$.{Key}') {Comparison.ToSqlNativeOperator()} '{value}'";
        }
    }
}
