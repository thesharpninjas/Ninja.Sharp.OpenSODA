// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionAttribute(string collectionName) : Attribute
    {
        public string CollectionName { get; set; } = collectionName;
    }
}
