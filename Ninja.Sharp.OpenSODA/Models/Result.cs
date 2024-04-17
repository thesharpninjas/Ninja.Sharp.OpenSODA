// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Models
{
    internal class Result<T> where T : class, new()
    {
        public ICollection<Item<T>> Items { get; set; } = [];
        public bool HasMore { get; set; }
        public int Count { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public int TotalResults { get; set; }
        public ICollection<Link> Links { get; set; } = [];
    }
    
}
