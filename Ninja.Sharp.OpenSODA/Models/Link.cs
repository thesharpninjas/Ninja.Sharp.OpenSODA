// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json.Serialization;

namespace Ninja.Sharp.OpenSODA.Models
{
    internal class Link
    {
        [JsonPropertyName("rel")]
        public string Rel { get; set; } = string.Empty;
        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;
    }
}
