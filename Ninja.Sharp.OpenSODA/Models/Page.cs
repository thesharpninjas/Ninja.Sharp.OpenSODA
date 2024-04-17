// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Enums;

namespace Ninja.Sharp.OpenSODA.Models
{
    public class Page
    {
        public int PageNumber { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 10;
        public string? OrderingPath { get; set; }
        public Ordering Ordering { get; set; } = Ordering.Ascending;
    }
}
