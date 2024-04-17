// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

namespace Ninja.Sharp.OpenSODA.Queries.Operations
{
    public class And : Or
    {
        protected override string SodaParameter => "$and";
        protected override string SqlParameter => "AND";
    }
}
