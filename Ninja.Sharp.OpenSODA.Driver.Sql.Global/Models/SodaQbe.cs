// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json.Serialization;

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Models
{
    internal class SodaQbe
    {
        [JsonPropertyName("$query")]
        public object? Query { get; set; }
        [JsonPropertyName("$orderby")]
        public object? OrderBy { get; set; }
    }

    internal class OrderByQbe
    {
        public string? Path { get; set; }
        public string Order { get; set; } = string.Empty;
    }
}
