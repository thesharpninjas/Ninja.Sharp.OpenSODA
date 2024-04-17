// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json.Serialization;

namespace Ninja.Sharp.OpenSODA.Models
{
    public class Item<T> where T : class, new()
    {
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("etag")]
        public string ETag { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public DateTime Created { get; set; }
        public T Value { get; set; } = new();
    }
}
