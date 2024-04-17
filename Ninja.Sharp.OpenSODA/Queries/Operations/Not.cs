// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json.Nodes;

namespace Ninja.Sharp.OpenSODA.Queries.Operations
{
    public class Not : Operation
    {
        internal override JsonObject GenerateQbeQuery(JsonObject obj)
        {
            if (Parameters.Count != 1)
            {
                throw new ArgumentException("-Not- must have a single parameter");
            }
            var jObj = Parameters.First().GenerateQbeQuery([]);
            obj.Add("$not", jObj);
            return obj;
        }

        internal override string GenerateSqlNativeQuery()
        {
            if (Parameters.Count != 1)
            {
                throw new ArgumentException("-Not- must have a single parameter");
            }
            return "!" + Parameters.First().GenerateSqlNativeQuery();
        }
    }
}
