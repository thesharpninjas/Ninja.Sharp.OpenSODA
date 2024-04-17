// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json.Serialization;

namespace Ninja.Sharp.OpenSODA.Models
{
    internal class CollectionsResult
    {
        [JsonPropertyName("items")]
        public ICollection<CollectionResult> Items { get; set; } = [];
        [JsonPropertyName("hasMore")]
        public bool HasMore { get; set; }
    }

}
