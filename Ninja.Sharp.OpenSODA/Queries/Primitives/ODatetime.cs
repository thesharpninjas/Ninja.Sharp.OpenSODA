// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Queries.Enums;
using System.Text.Json.Nodes;

namespace Ninja.Sharp.OpenSODA.Queries.Primitives
{
    public class ODatetime(string key, DateTime value, Compare operationType = Compare.Equals) : SodaPrimitive(key, operationType)
    {
        private readonly DateTime value = value;

        internal override JsonObject GenerateQbeQuery(JsonObject obj)
        {
            var myValue = value.ToString("O");

            JsonObject item = new()
            {
                { Comparison.ToQbeOperator(), myValue }
            };

            JsonObject timestamp = new()
            {
                { "$timestamp", item }
            };

            obj.Add(Key, timestamp);
            return obj;
        }

        internal override string GenerateSqlNativeQuery()
        {
            return $"json_value(\"JSON_DOCUMENT\", '$.{Key}') {Comparison.ToSqlNativeOperator()} '{value:O}'";
        }
    }
}
