// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json.Serialization;

namespace Ninja.Sharp.OpenSODA.Models
{
    internal class CollectionResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("properties")]
        public CollectionProperties Properties { get; set; } = new();
        [JsonPropertyName("link")]
        public ICollection<Link> Links { get; set; } = [];
    }

}
