// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Models
{
    internal class CollectionProperties
    {
        public string SchemaName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public bool ReadOnly { get; set; }
        public CollectionColumnProperties KeyColumn { get; set; } = new();
        public CollectionColumnProperties ContentColumn { get; set; } = new();
        public CollectionColumnProperties VersionColumn { get; set; } = new();
        public CollectionColumnProperties LastModifiedColumn { get; set; } = new();
        public CollectionColumnProperties CreationTimeColumn { get; set; } = new();
    }
    
}
