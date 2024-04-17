// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Queries.Enums;
using System.Text.Json.Nodes;

namespace Ninja.Sharp.OpenSODA.Queries.Primitives
{
    public class OString(string key, string value, Compare operationType = Compare.Equals) : SodaPrimitive(key, operationType)
    {
        private readonly string value = value;

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
            string escapedValue = value.Replace("-", "\\-");
            return Comparison switch
            {
                Compare.Equals or Compare.Contains => $"json_textcontains(\"JSON_DOCUMENT\", '$.{Key}', '{escapedValue}')",
                _ => $"json_value(\"JSON_DOCUMENT\", '$.{Key}') {Comparison.ToSqlNativeOperator()} '{escapedValue}'",
            };
        }
    }
}
