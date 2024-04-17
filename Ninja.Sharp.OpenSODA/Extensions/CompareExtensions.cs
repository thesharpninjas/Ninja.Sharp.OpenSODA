// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Ninja.Sharp.OpenSODA.Queries.Enums;

namespace Ninja.Sharp.OpenSODA.Extensions
{
    internal static class CompareExtensions
    {
        public static string ToSqlNativeOperator(this Compare type)
        {
            return type switch
            {
                Compare.Equals => "=",
                Compare.NotEquals => "<>",
                Compare.GreaterThan => ">",
                Compare.LessThan => "<",
                Compare.GreaterThanOrEquals => ">=",
                Compare.LessThanOrEquals => "<=",
                _ => throw new ArgumentException("invalid comparison type"),
            };
        }

        public static string ToQbeOperator(this Compare type)
        {
            return type switch
            {
                Compare.Equals => "$eq",
                Compare.NotEquals => "$ne",
                Compare.GreaterThan => "$gt",
                Compare.LessThan => "$lt",
                Compare.GreaterThanOrEquals => "$gte",
                Compare.LessThanOrEquals => "$lte",
                Compare.HasSubstring => "$hasSubstring",
                Compare.In => "$in",
                Compare.Instr => "$instr",
                Compare.Like => "$like",
                Compare.NotIn => "$nin",
                Compare.Regex => "$regex",
                Compare.StartsWith => "$startsWith",
                Compare.All => "$all",
                Compare.Between => "$between",
                Compare.Exists => "$exists",
                _ => throw new ArgumentException("invalid comparison type"),
            };
        }
    }
}
