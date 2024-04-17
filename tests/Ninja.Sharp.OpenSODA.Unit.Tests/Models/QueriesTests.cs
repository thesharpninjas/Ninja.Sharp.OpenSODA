using AutoFixture.Xunit2;
using Ninja.Sharp.OpenSODA.Extensions;
using Ninja.Sharp.OpenSODA.Queries.Enums;
using Ninja.Sharp.OpenSODA.Queries.Operations;
using Ninja.Sharp.OpenSODA.Queries.Primitives;
using System.Text.Json.Nodes;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Models
{
    public class QueriesTests()
    {
        [Theory]
        [InlineAutoData]
        public void ODatetime_Generate_Ok(string key, DateTime value, Compare compare)
        {
            ODatetime oDatetime = new(key, value, compare);
            JsonObject obj = [];

            JsonObject result = oDatetime.GenerateQbeQuery(obj);

            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData(Compare.Equals)]
        [InlineAutoData(Compare.NotEquals)]
        [InlineAutoData(Compare.GreaterThan)]
        [InlineAutoData(Compare.LessThan)]
        [InlineAutoData(Compare.GreaterThanOrEquals)]
        [InlineAutoData(Compare.LessThanOrEquals)]
        [InlineAutoData(Compare.HasSubstring)]
        [InlineAutoData(Compare.In)]
        [InlineAutoData(Compare.Instr)]
        [InlineAutoData(Compare.Like)]
        [InlineAutoData(Compare.NotIn)]
        [InlineAutoData(Compare.Regex)]
        [InlineAutoData(Compare.StartsWith)]
        [InlineAutoData(Compare.All)]
        [InlineAutoData(Compare.Between)]
        [InlineAutoData(Compare.Exists)]
        public void Compare_ToSodaOperator_Ok(Compare compare)
        {
            string result = compare.ToQbeOperator();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineAutoData(Compare.Equals)]
        [InlineAutoData(Compare.NotEquals)]
        [InlineAutoData(Compare.GreaterThan)]
        [InlineAutoData(Compare.LessThan)]
        [InlineAutoData(Compare.GreaterThanOrEquals)]
        [InlineAutoData(Compare.LessThanOrEquals)]
        public void Compare_ToSqlOperator_Ok(Compare compare)
        {
            string result = compare.ToSqlNativeOperator();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineAutoData((Compare)(-1))]
        [InlineAutoData((Compare)(100))]
        public void Compare_ToOperator_Ko(Compare compare)
        {
            Assert.Throws<ArgumentException>(() => compare.ToQbeOperator());
        }

        [Theory]
        [InlineAutoData((Compare)(-1))]
        [InlineAutoData((Compare)100)]
        [InlineAutoData(Compare.Like)]
        public void Compare_ToSqlOperator_Ko(Compare compare)
        {
            Assert.Throws<ArgumentException>(() => compare.ToSqlNativeOperator());
        }

        [Theory]
        [InlineAutoData]
        public void And_Generate_Ok(string key1, string value1, Compare compare1, string key2, string value2, Compare compare2)
        {
            And and = new();
            JsonObject obj = [];

            and.With(new OString(key1, value1, compare1));
            and.With(new OString(key2, value2, compare2));

            JsonObject resultSoda = and.GenerateQbeQuery(obj);
            string resultSql = and.GenerateSqlNativeQuery();

            Assert.NotNull(resultSoda);
            Assert.NotNull(resultSql);
        }

        [Theory]
        [InlineAutoData]
        public void And_GenerateWithUpper_Ok(string key1, string value1, Compare compare1, string key2, string value2, Compare compare2)
        {
            And and = new();
            JsonObject obj = [];

            and.With(new OUpperString(key1, value1, compare1));
            and.With(new OUpperString(key2, value2, compare2));

            JsonObject resultSoda = and.GenerateQbeQuery(obj);
            string resultSql = and.GenerateSqlNativeQuery();

            Assert.NotNull(resultSoda);
            Assert.NotNull(resultSql);
        }

        [Theory]
        [InlineAutoData]
        public void And_GenerateWithDateTime_Ok(string key1, DateTime value1, Compare compare1, string key2, DateTime value2, Compare compare2)
        {
            And and = new();
            JsonObject obj = [];

            and.With(new ODatetime(key1, value1, compare1));
            and.With(new ODatetime(key2, value2, compare2));

            JsonObject resultSoda = and.GenerateQbeQuery(obj);
            string resultSql = and.GenerateSqlNativeQuery();

            Assert.NotNull(resultSoda);
            Assert.NotNull(resultSql);
        }

        [Fact]
        public void And_Generate_Ko()
        {
            And and = new();
            JsonObject obj = [];

            Assert.Throws<ArgumentException>(() => and.GenerateQbeQuery(obj));
        }

        [Theory]
        [InlineAutoData]
        public void OInt_Generate_Ok(string key, int value, Compare compare)
        {
            OInt oInt = new(key, value, compare);
            JsonObject obj = [];

            JsonObject resultSoda = oInt.GenerateQbeQuery(obj);
            string resultSql = oInt.GenerateSqlNativeQuery();

            Assert.NotNull(resultSoda);
            Assert.NotNull(resultSql);
        }

        [Theory]
        [InlineAutoData]
        public void Not_Generate_Ok(string key, string value, Compare compare)
        {
            Not not = new();
            JsonObject obj = [];

            not.With(new OString(key, value, compare));

            JsonObject resultSoda = not.GenerateQbeQuery(obj);
            string resultSql = not.GenerateSqlNativeQuery();

            Assert.NotNull(resultSoda);
            Assert.NotNull(resultSql);
        }

        [Fact]
        public void Not_Generate_Ko_NoParams()
        {
            Not not = new();
            JsonObject obj = [];

            Assert.Throws<ArgumentException>(() => not.GenerateQbeQuery(obj));
        }

        [Fact]
        public void Not_GenerateQuery_Ko_NoParams()
        {
            Not not = new();

            Assert.Throws<ArgumentException>(not.GenerateSqlNativeQuery);
        }

        [Theory]
        [InlineAutoData]
        public void Not_Generate_Ko_MoreParams(string[] keys, string[] values, Compare[] compares)
        {
            Not not = new();
            JsonObject obj = [];

            for (int i = 0; i < keys.Length; ++i)
            {
                not.With(new OString(keys[i], values[i], compares[i]));
            }

            Assert.Throws<ArgumentException>(() => not.GenerateQbeQuery(obj));
        }

        [Theory]
        [InlineAutoData]
        public void Or_Generate_Ok(string key1, string value1, Compare compare1, string key2, string value2, Compare compare2)
        {
            Or or = new();
            JsonObject obj = [];

            or.With(new OString(key1, value1, compare1));
            or.With(new OString(key2, value2, compare2));

            JsonObject resultSoda = or.GenerateQbeQuery(obj);
            string resultSql = or.GenerateSqlNativeQuery();

            Assert.NotNull(resultSoda);
            Assert.NotNull(resultSql);
        }

        [Fact]
        public void Or_Generate_Ko()
        {
            Or or = new();
            JsonObject obj = [];

            Assert.Throws<ArgumentException>(() => or.GenerateQbeQuery(obj));
        }
    }
}
