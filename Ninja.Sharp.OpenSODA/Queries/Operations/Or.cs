// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using System.Text;
using System.Text.Json.Nodes;

namespace Ninja.Sharp.OpenSODA.Queries.Operations
{
    public class Or : Operation
    {
        internal override JsonObject GenerateQbeQuery(JsonObject obj)
        {
            if (Parameters.Count == 0)
            {
                throw new ArgumentException($"-{SodaParameter}- must have some parameters");
            }

            var array = new JsonArray();
            foreach (var item in Parameters)
            {
                var jObj = item.GenerateQbeQuery([]);
                array.Add(jObj);
            }
            obj.Add(SodaParameter, array);
            return obj;
        }

        protected virtual string SodaParameter => "$or";
        protected virtual string SqlParameter => "OR";

        internal override string GenerateSqlNativeQuery()
        {
            StringBuilder sqlQueryBuilder = new();
            sqlQueryBuilder.Append('(');
            foreach (var item in Parameters)
            {
                var query = item.GenerateSqlNativeQuery();
                sqlQueryBuilder.Append(query);
                sqlQueryBuilder.Append($" {SqlParameter} ");
            }

            var sqlQuery = sqlQueryBuilder.ToString().TrimEnd();
            sqlQuery = sqlQuery.Remove(sqlQuery.LastIndexOf(SqlParameter));
            sqlQuery = sqlQuery.TrimEnd();

            return sqlQuery + ")";
        }
    }
}
