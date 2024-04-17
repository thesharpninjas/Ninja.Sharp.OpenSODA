// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Queries.Enums;
using Ninja.Sharp.OpenSODA.Queries.Operations;
using System.Text.Json;

namespace Ninja.Sharp.OpenSODA.Queries.Primitives
{
    public abstract class SodaPrimitive(string key, Compare comparison = Compare.Equals) : Operation
    {
        public string Key { get; private set; } = JsonNamingPolicy.CamelCase.ConvertName(key);
        public Compare Comparison { get; private set; } = comparison;
    }
}
