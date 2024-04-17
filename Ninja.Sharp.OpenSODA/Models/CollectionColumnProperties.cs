// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Models
{
    internal class CollectionColumnProperties
    {
        public string Name { get; set; } = string.Empty;
        public string SqlType { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int MaxLength { get; set; }
        public string Path { get; set; } = string.Empty;
        public string AssignmentMethod { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
    }
}
