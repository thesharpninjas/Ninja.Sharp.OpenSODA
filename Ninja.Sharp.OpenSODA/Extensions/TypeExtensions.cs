// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Attributes;
using Ninja.Sharp.OpenSODA.Exceptions;

namespace Ninja.Sharp.OpenSODA.Extensions
{
    internal static class TypeExtensions
    {
        public static string CollectionName(this Type userType, string? collection = null)
        {
            if (!string.IsNullOrWhiteSpace(collection))
            {
                if (collection.All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    return collection.ToString();
                }
                throw new SodaConfigurationException("[SODA] Collection name must have only alphanumeric characters or the _ character.");
            }

            CollectionAttribute[] customAttributes = (CollectionAttribute[])userType.GetCustomAttributes(typeof(CollectionAttribute), true);
            if (customAttributes.Length > 0)
            {
                CollectionAttribute myAttribute = customAttributes[0];
                // controllo che siano solo lettere, così da evitare giochetti sql col nome della tabella, visto che non posso metterla sempre nei parametri, purtroppo
                if (!string.IsNullOrWhiteSpace(myAttribute.CollectionName) && myAttribute.CollectionName.All(c => char.IsLetterOrDigit(c) || c == '_'))
                {
                    return myAttribute.CollectionName;
                }
            }
            return userType.Name;
        }
    }
}
