// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text.Json.Nodes;

namespace Ninja.Sharp.OpenSODA.Queries.Operations
{
    public abstract class Operation
    {
        protected ICollection<Operation> Parameters { get; private set; } = [];

        internal abstract JsonObject GenerateQbeQuery(JsonObject obj);

        internal abstract string GenerateSqlNativeQuery();

        public Operation With(Operation operation)
        {
            Parameters.Add(operation);
            return this;
        }
    }
}
