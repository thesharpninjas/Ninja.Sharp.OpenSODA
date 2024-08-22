// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ninja.Sharp.OpenSODA.Extensions
{
    internal static class ConversionExtensions
    {

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        internal static string Serialize<T>(this T item)
        {
            if (item == null)
            {
                var requestedTypeName = typeof(T).Name;
                throw new ArgumentNullException(requestedTypeName, "The item to serialize cannot be null.");
            }
            
            var myOptions = options;
            return JsonSerializer.Serialize(item, myOptions);
        }

        internal static T Deserialize<T>(this string item)
        {
            var requestedTypeName = typeof(T).Name;
            if (string.IsNullOrWhiteSpace(item))
            {
                throw new ArgumentException("Cannot convert to " + requestedTypeName);
            }

            try
            {
                var myItem = JsonSerializer.Deserialize<T>(item, options);
                return myItem == null ? throw new ArgumentException("Cannot convert to " + requestedTypeName) : myItem;
            }
            catch (Exception)
            {
                throw new ArgumentException("Cannot convert to " + requestedTypeName);
            }

        }
    }
}
