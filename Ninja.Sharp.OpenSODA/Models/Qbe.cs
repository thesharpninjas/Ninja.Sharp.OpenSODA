// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json.Serialization;

namespace Ninja.Sharp.OpenSODA.Models
{
    internal class Qbe
    {
        [JsonPropertyName("$query")]
        public object? Query { get; set; }
        [JsonPropertyName("$orderby")]
        public object? OrderBy { get; set; }
    }
}
